using log4net;
using Project.Models.Domain;
using Project.Models.Requests;
using Project.Models.Responses;
using Project.Models.ViewModels;
using Project.Services;
using Project.Services.Interfaces;
using Project.Services.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;

namespace Project.Web.Controllers.Api
{
    [RoutePrefix("api/forum")]
    public class ForumController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ForumController));
        private IForumService _service;
        private IPrincipal _principal;
        private IFileUploadService _fileService;
        private IConfigSettingsService _configServices;
        private IProfanityService _profService;

        [Route("comments/get"), HttpPost]
        public HttpResponseMessage GetCommentsByForumId(ForumPageRequest model)
        {
            try
            {
                //Checks that the passed in model is valid
                if (ModelState.IsValid)
                {
                    ItemResponse<ForumCommentsViewModel> resp = new ItemResponse<ForumCommentsViewModel>();
                    //Gets the list of forum comments
                    resp.Item = _service.GetAllCommentsByForumId(model);
                    //Loops through each comment and convert the file ids into the correct file path to show on the page
                    foreach (ForumComment comment in resp.Item.Comments)
                    {
                        string serverPath = _configServices.GetConfigValueByName("AWS:BaseURL").ConfigValue;
                        comment.UploadedFiles = new List<UploadedFile>();
                        foreach(int fileId in comment.FileIds)
                        {
                            UploadedFile upFile = _fileService.GetById(fileId);
                            string filePath = Path.Combine(serverPath, upFile.SystemFileName);
                            upFile.SystemFileName = filePath;
                            comment.UploadedFiles.Add(upFile);
                        }
                        //Gets the user's profile picture
                        comment.Person.ProfilePic = Path.Combine(serverPath, comment.Person.ProfilePic);                        
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error getting comments with forum Id: " + model.ForumId, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("currentUser")]
        public HttpResponseMessage GetForumByPersonId()
        {
            //Gets the current user's id from the cookie
            int id = _principal.Identity.GetCurrentUser().Id;
            try
            {
                ItemsResponse<ForumStatusViewModel> resp = new ItemsResponse<ForumStatusViewModel>();
                //Gets the active and closed forums this person is part of
                resp.Items = _service.GetAllForumsByPersonId(id);
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error getting forums with person Id: " + id, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("comments"), HttpPost]
        public HttpResponseMessage PostComment(ForumCommentAddRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Gets the currently logged in user
                    var user = _principal.Identity.GetCurrentUser();
                    model.PersonId = user.Id;
                    model.ModifiedBy = user.Name;
                    //Calls the profanity service to censor any curse words in the text
                    model.Text = _profService.Cleanse(model.Text);
                    ItemResponse<int> resp = new ItemResponse<int>();
                    //Posts the forum comment into the database
                    resp.Item = _service.PostForumComment(model);
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error posting new comment", ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("comments/{id:int}"), HttpDelete]
        public HttpResponseMessage DeleteComment(int id)
        {
            try
            {
                //Deletes the comment with the passed in id
                _service.DeleteForumComment(id);
                SuccessResponse resp = new SuccessResponse();
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error deleting comment: " + id, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("comments/report"), HttpPost]
        public HttpResponseMessage ReportComment(ForumReportAddRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Gets the current user from the cookie
                    var user = _principal.Identity.GetCurrentUser();
                    model.ReporterId = user.Id;
                    model.CreatedBy = user.Name;
                    //Filters the report text to censor any curse words
                    model.ReportText = _profService.Cleanse(model.ReportText);
                    ItemResponse<int> resp = new ItemResponse<int>();
                    //Sends the report to the database
                    resp.Item = _service.ReportComment(model);
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error reporting comment", ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("comments/report/selectTotals"), HttpPost]
        public HttpResponseMessage GetReports(ForumReportPageModel model)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    ItemResponse<ForumReportsCommentsList> resp = new ItemResponse<ForumReportsCommentsList>();
                    //Gets all the top level reports
                    resp.Item = _service.GetReportTotals(model);
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error getting reported comments", ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("comments/report/selectComment"), HttpPost]
        public HttpResponseMessage GetReportsByComment(ForumReportComPage model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ItemsResponse<ForumReportModel> resp = new ItemsResponse<ForumReportModel>();
                    //Gets the reports for a specific comment
                    resp.Items = _service.GetReportsByComment(model);
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error getting reports by comment", ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("comments/report/{id:int}"), HttpDelete]
        public HttpResponseMessage DeleteReport(int id)
        {
            try
            {
                //Deletes a report
                _service.DeleteReport(id);
                SuccessResponse resp = new SuccessResponse();
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch(Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error deleting report: " + id, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [Route("getall"), HttpPost]
        public HttpResponseMessage GetAllForums(ForumSelectModel model)
        {
            try
            {
                ItemResponse<ForumReturnList> resp = new ItemResponse<ForumReturnList>();
                //Gets all the forums that matches the passed in model
                resp.Item = _service.GetAllForums(model);
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error Getting Forums", ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("{statusId:int}"), HttpPost]
        public HttpResponseMessage GetByStatusId(ForumSelectModel model, int statusId)
        {
            try
            {
                ItemResponse<ForumReturnList> resp = new ItemResponse<ForumReturnList>();
                //Gets a list of forums with the passed in status id
                resp.Item = _service.GetByStatusId(model, statusId);
                return Request.CreateResponse(HttpStatusCode.OK, resp);
            }
            catch (System.Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error getting forums with status id: " + statusId, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route("{id:int}"), HttpPut]
        public HttpResponseMessage Put(int id, ForumUpdateRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.ModifiedBy = _principal.Identity.GetCurrentUser().Name;
                    //Updates the forum with the passed in id
                    _service.Update(model);
                    SuccessResponse resp = new SuccessResponse();
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (System.Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error updating forum id: " + id, ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [Route, HttpPost]
        public HttpResponseMessage Post(ForumAddRequest model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.ModifiedBy = _principal.Identity.GetCurrentUser().Name;
                    //Creates a new forum
                    int id = _service.Insert(model);
                    ItemResponse<int> resp = new ItemResponse<int>();
                    resp.Item = id;
                    return Request.CreateResponse(HttpStatusCode.OK, resp);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            catch (System.Exception ex)
            {
                //Log any exception that occurs
                log.Error("Error Creating new Forum", ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        public ForumController(IForumService service, IPrincipal principal, IFileUploadService fileService, IConfigSettingsService configServices, IProfanityService profService)
        {
            _service = service;
            _principal = principal;
            _fileService = fileService;
            _configServices = configServices;
            _profService = profService;
        }
    }
}
