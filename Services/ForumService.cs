using Project.Data;
using Project.Models.Domain;
using Project.Models.Requests;
using Project.Models.ViewModels;
using Project.Services.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Project.Services
{
    public class ForumService : BaseService, IForumService
    {
        public ForumCommentsViewModel GetAllCommentsByForumId(ForumPageRequest model)
        {
            //Creates a dictionary of forum comments in order to insert data directly into a specific comment
            Dictionary<int, ForumComment> CommentDict = new Dictionary<int, ForumComment>();
            ForumCommentsViewModel fcModel = new ForumCommentsViewModel();
            //Initializes the two lists so we can insert things into them
            fcModel.Comments = new List<ForumComment>();
            fcModel.TeamMembers = new List<ForumCommentPerson>();
            //Runs the stored procedure
            DataProvider.ExecuteCmd(
                "Forum_Comment_SelectByForumId",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@ForumId", model.ForumId);
                    paramCol.AddWithValue("@PageSize", model.PageSize);
                    paramCol.AddWithValue("@PageNum", model.PageNum);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    //Switches between each sql select statement
                    switch (set)
                    {
                        //Gets the project information
                        case 0:
                            fcModel.Name = reader.GetSafeString(0);
                            fcModel.Description = reader.GetSafeString(1);
                            break;
                        //Gets the total number of comments
                        case 1:
                            fcModel.TotalComments = reader.GetSafeInt32(0);
                            break;
                        //Gets the list of comments
                        case 2:
                            ForumComment fc = MapComment(reader);
                            fc.FileIds = new List<int>();
                            CommentDict.Add(fc.Id, fc);
                            fcModel.Comments.Add(fc);
                            break;
                        //Gets the list of files and adds them to their comment
                        case 3:
                            int fCommId = reader.GetSafeInt32(0);
                            int fileId = reader.GetSafeInt32(1);
                            CommentDict[fCommId].FileIds.Add(fileId);
                            break;
                        //Gets the information for each person and adds them to their comment
                        case 4:
                            ForumCommentPersonFull cp = new ForumCommentPersonFull();
                            int index = 0;
                            int cpId = reader.GetSafeInt32(index++);
                            cp.FirstName = reader.GetSafeString(index++);
                            cp.LastName = reader.GetSafeString(index++);
                            cp.Role = reader.GetSafeString(index++);
                            cp.IsCaptain = reader.GetSafeBool(index++);
                            cp.ProfilePic = reader.GetSafeString(index++);
                            CommentDict[cpId].Person = cp;
                            break;
                        //Gets comment quotes and adds them to their comment
                        case 5:
                            ForumCommentQuote cq = new ForumCommentQuote();
                            int idx = 0;
                            int Id = reader.GetSafeInt32(idx++);
                            cq.Text = reader.GetSafeString(idx++);
                            cq.FirstName = reader.GetSafeString(idx++);
                            cq.LastName = reader.GetSafeString(idx++);
                            cq.CreatedDate = reader.GetSafeDateTime(idx++);
                            CommentDict[Id].Quote = cq;
                            break;
                        //Gets the list of team members for this forum
                        case 6:
                            ForumCommentPerson p = new ForumCommentPerson();
                            int i = 0;
                            p.FirstName = reader.GetSafeString(i++);
                            p.LastName = reader.GetSafeString(i++);
                            p.IsCaptain = reader.GetSafeBool(i++);
                            fcModel.TeamMembers.Add(p);
                            break;
                    }
                }
            );
            return fcModel;
        }

        public List<ForumStatusViewModel> GetAllForumsByPersonId(int personId)
        {
            List<ForumStatusViewModel> list = new List<ForumStatusViewModel>();
            //Creates the dictionaries for the statuses and the forums, so we can insert data to the correct location
            Dictionary<int, ForumStatusViewModel> statusDict = new Dictionary<int, ForumStatusViewModel>();
            Dictionary<int, ForumViewModel> forumDict = new Dictionary<int, ForumViewModel>();
            //Runs the stored procedure
            DataProvider.ExecuteCmd(
                "Forum_SelectByPersonId",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@PersonId", personId);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    //Switches between the sql select statements
                    switch (set)
                    {
                        //Gets the list of statuses to show to the user
                        case 0:
                            ForumStatusViewModel sModel = new ForumStatusViewModel();
                            sModel.Forums = new List<ForumViewModel>();
                            int id = reader.GetSafeInt32(0);
                            sModel.Description = reader.GetSafeString(1);
                            statusDict.Add(id, sModel);
                            list.Add(sModel);
                            break;
                        //Gets the list of forums and assigns them to a status
                        case 1:
                            ForumViewModel fModel = new ForumViewModel();
                            int index = 0;
                            fModel.Id = reader.GetSafeInt32(index++);
                            fModel.Name = reader.GetSafeString(index++);
                            fModel.Description = reader.GetSafeString(index++);
                            int statusId = reader.GetSafeInt32(index++);
                            fModel.ModifiedDate = reader.GetSafeDateTime(index++);
                            statusDict[statusId].Forums.Add(fModel);
                            forumDict.Add(fModel.Id, fModel);
                            break;
                        //Gets the total amount of comments for each forum
                        case 2:
                            int fId = reader.GetSafeInt32(0);
                            int total = reader.GetSafeInt32(1);
                            forumDict[fId].TotalComments = total;
                            break;
                    }
                }
            );
            return list;
        }

        public int PostForumComment(ForumCommentAddRequest model)
        {
            int id = 0;
            //Executes the stored procedure
            DataProvider.ExecuteNonQuery(
                "Forum_Comments_Insert",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    //Creates the Id output parameter
                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@Id";
                    param.SqlDbType = SqlDbType.Int;
                    param.Direction = ParameterDirection.Output;
                    paramCol.Add(param);

                    paramCol.AddWithValue("@ForumId", model.ForumId);
                    paramCol.AddWithValue("@PersonId", model.PersonId);
                    paramCol.AddWithValue("@QuoteId", model.QuoteId);
                    paramCol.AddWithValue("@Text", model.Text);
                    paramCol.AddWithValue("@ModifiedBy", model.ModifiedBy);
                    paramCol.AddWithValue("@Files", CreateFileTable(model.Files));
                },
                returnParameters: delegate (SqlParameterCollection paramCol)
                {
                    id = (int)paramCol["@Id"].Value;
                }
            );
            return id;
        }

        public void DeleteForumComment(int id)
        {
            //Runs the stored procedure to delete a forum comment
            DataProvider.ExecuteNonQuery(
                "Forum_Comments_Delete",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@Id", id);
                }
            );
        }

        public int ReportComment(ForumReportAddRequest model)
        {
            int id = 0;
            //Runs the stored procedure to report a forum comment
            DataProvider.ExecuteNonQuery(
                "Forum_Reports_Insert",
                inputParamMapper: delegate(SqlParameterCollection paramCol)
                {
                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@Id";
                    param.SqlDbType = SqlDbType.Int;
                    param.Direction = ParameterDirection.Output;
                    paramCol.Add(param);
                    paramCol.AddWithValue("@CommentId", model.CommentId);
                    paramCol.AddWithValue("@PersonId", model.ReporterId);
                    paramCol.AddWithValue("@ReportText", model.ReportText);
                    paramCol.AddWithValue("@CreatedBy", model.CreatedBy);
                },
                returnParameters: delegate(SqlParameterCollection paramCol)
                {
                    id = (int)paramCol["@Id"].Value;
                }
            );
            return id;
        }

        public ForumReportsCommentsList GetReportTotals(ForumReportPageModel model)
        {
            ForumReportsCommentsList returnModel = new ForumReportsCommentsList();
            returnModel.List = new List<ForumReportCommentModel>();
            //Runs the stored procedure
            DataProvider.ExecuteCmd(
                "Forum_Reports_SelectAll",
                inputParamMapper: delegate(SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@PageSize", model.PageSize);
                    paramCol.AddWithValue("@PageNum", model.PageNum);
                },
                singleRecordMapper: delegate(IDataReader reader, short set)
                {
                    //Switches between the sql select statements
                    switch (set)
                    {
                        //Gets the list of reported comments
                        case 0:
                            ForumReportCommentModel comment = new ForumReportCommentModel();
                            int index = 0;
                            comment.CommentId = reader.GetSafeInt32(index++);
                            comment.ForumId = reader.GetSafeInt32(index++);
                            comment.Text = reader.GetSafeString(index++);
                            comment.Total = reader.GetSafeInt32(index++);
                            returnModel.List.Add(comment);
                            break;
                        //Gets the total list of reported comments
                        case 1:
                            returnModel.Total = reader.GetSafeInt32(0);
                            break;
                    }
                    
                }
            );
            return returnModel;
        }

        public List<ForumReportModel> GetReportsByComment(ForumReportComPage model)
        {
            List<ForumReportModel> list = new List<ForumReportModel>();
            //Runs the stored procedure
            DataProvider.ExecuteCmd(
                "Forum_Reports_SelectByCommentId",
                inputParamMapper: delegate(SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@CommentId", model.CommentId);
                    paramCol.AddWithValue("@PageSize", model.PageSize);
                    paramCol.AddWithValue("@PageNum", model.PageNum);
                },
                singleRecordMapper: delegate(IDataReader reader, short set)
                {
                    //Loops through the select, getting each report for a specific comment
                    ForumReportModel report = new ForumReportModel();
                    int index = 0;
                    report.Id = reader.GetSafeInt32(index++);
                    report.FirstName = reader.GetSafeString(index++);
                    report.LastName = reader.GetSafeString(index++);
                    report.ReportText = reader.GetSafeString(index++);
                    report.CreatedDate = reader.GetSafeDateTime(index++);
                    list.Add(report);
                }
            );

            return list;
        }

        public void DeleteReport(int id)
        {
            //Deletes a report from the db
            DataProvider.ExecuteNonQuery(
                "Forum_Reports_Delete",
                inputParamMapper: delegate(SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@Id", id);
                }
            );
        }

        public int Insert(ForumAddRequest model)
        {
            int id = 0;
            //Runs the stored procedure to create a new forum
            this.DataProvider.ExecuteNonQuery(
                "Forum_Insert",
                inputParamMapper: delegate (SqlParameterCollection forumParamCol)
                {
                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@Id";
                    param.SqlDbType = System.Data.SqlDbType.Int;
                    param.Direction = System.Data.ParameterDirection.Output;
                    forumParamCol.Add(param);

                    forumParamCol.AddWithValue("@ProjectId", model.ProjectId);
                    forumParamCol.AddWithValue("@StatusId", model.StatusId);
                    forumParamCol.AddWithValue("@ModifiedBy", model.ModifiedBy);
                },
                returnParameters: delegate (SqlParameterCollection forumParamCol)
                {
                    id = (int)forumParamCol["@Id"].Value;
                }
            );
            return id;
        }

        public ForumReturnList GetByStatusId(ForumSelectModel model, int statusId)
        {
            //Dictionary used to insert data into a specific forum
            Dictionary<int, ForumModel> statusDict = new Dictionary<int, ForumModel>();
            ForumReturnList returnModel = new ForumReturnList();
            returnModel.ForumList = new List<ForumModel>();
            //Runs stored proc selects all forums with a specific status id
            DataProvider.ExecuteCmd(
                "Forum_SelectByStatusId",
                inputParamMapper: delegate (SqlParameterCollection forumParamCol)
                {
                    forumParamCol.AddWithValue("@StatusId", statusId);
                    forumParamCol.AddWithValue("@PageSize", model.PageSize);
                    forumParamCol.AddWithValue("@PageNum", model.PageNum);
                    forumParamCol.AddWithValue("@NameFilter", model.NameFilter);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    //Switches between the different sql selects
                    switch (set)
                    {
                        //Gets all the forums
                        case 0:
                            ForumModel viewModel = new ForumModel();
                            int index = 0;
                            viewModel.Id = reader.GetSafeInt32(index++);
                            viewModel.StatusId = reader.GetSafeInt32(index++);
                            viewModel.ProjectId = reader.GetSafeInt32(index++);
                            viewModel.Status = reader.GetSafeString(index++);
                            viewModel.Name = reader.GetSafeString(index++);
                            viewModel.Description = reader.GetSafeString(index++);
                            viewModel.CreatedDate = reader.GetSafeDateTime(index++);
                            viewModel.ModifiedDate = reader.GetSafeDateTime(index++);
                            viewModel.ModifiedBy = reader.GetSafeString(index++);
                            returnModel.ForumList.Add(viewModel);
                            statusDict.Add(viewModel.Id, viewModel);
                            break;
                        //Gets the total number of forums for this status
                        case 1:
                            returnModel.ForumTotal = reader.GetSafeInt32(0);
                            break;
                        //Gets the total number of comments for each forum
                        case 2:
                            int indexx = 0;
                            int sId = reader.GetSafeInt32(indexx++);
                            int total = reader.GetSafeInt32(indexx);
                            statusDict[sId].TotalComments = total;
                            break;
                    }
                }
            );
            return returnModel;
        }

        public ForumReturnList GetAllForums(ForumSelectModel model)
        {
            ForumReturnList returnModel = new ForumReturnList();
            returnModel.ForumList = new List<ForumModel>();
            //Runs the stored proc to get all the forums
            DataProvider.ExecuteCmd(
                "Forum_SelectAll",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@PageSize", model.PageSize);
                    paramCol.AddWithValue("@PageNum", model.PageNum);
                    paramCol.AddWithValue("@NameFilter", model.NameFilter);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    switch (set)
                    {
                        //Gets the list of forums
                        case 0:
                            ForumModel viewModel = new ForumModel();
                            {
                                int index = 0;
                                viewModel.Id = reader.GetSafeInt32(index++);
                                viewModel.StatusId = reader.GetSafeInt32(index++);
                                viewModel.ProjectId = reader.GetSafeInt32(index++);
                                viewModel.Status = reader.GetSafeString(index++);
                                viewModel.Name = reader.GetSafeString(index++);
                                viewModel.Description = reader.GetSafeString(index++);
                                viewModel.CreatedDate = reader.GetSafeDateTime(index++);
                                viewModel.ModifiedDate = reader.GetSafeDateTime(index++);
                                viewModel.ModifiedBy = reader.GetSafeString(index++);
                                returnModel.ForumList.Add(viewModel);
                            }
                            break;
                        //Gets the total amount of forums
                        case 1:
                            returnModel.ForumTotal = reader.GetSafeInt32(0);
                            break;
                    }
                }
            );
            return returnModel;
        }

        public void Update(ForumUpdateRequest model)
        {
            //Runs the stored proc to update a forum
            this.DataProvider.ExecuteNonQuery(
             "Forum_Update",
             inputParamMapper: delegate (SqlParameterCollection forumParamCol)
             {
                 forumParamCol.AddWithValue("@Id", model.Id);
                 forumParamCol.AddWithValue("@StatusId", model.StatusId);
                 forumParamCol.AddWithValue("@ModifiedBy", model.ModifiedBy);
             }
           );
        }

        private ForumComment MapComment(IDataReader reader)
        {
            //Maps a sql row to a forum comment model
            int index = 0;
            ForumComment fc = new ForumComment();
            fc.Id = reader.GetSafeInt32(index++);
            fc.Text = reader.GetSafeString(index++);
            fc.CreatedDate = reader.GetSafeDateTime(index++);
            fc.ModifiedDate = reader.GetSafeDateTime(index++);
            fc.ModifiedBy = reader.GetSafeString(index++);
            return fc;
        }

        private static DataTable CreateFileTable(IEnumerable<int> fileList)
        {
            //Creates a table type of file ids add to sql db
            DataTable table = new DataTable();
            table.Columns.Add("FileId", typeof(int));
            foreach (int fileId in fileList)
            {
                table.Rows.Add(fileId);
            }
            return table;
        }
    }
}
