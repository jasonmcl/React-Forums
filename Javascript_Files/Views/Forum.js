import React from 'react';
import {Link} from 'react-router';
import axios from 'axios';
import CommentList from '../Components/commentList';
import CommentReport from '../Components/commentReport';
import File from '../Components/fileupload/files';
import ForumFile from '../Components/forumFile';
import MemberDropdown from '../Components/memberDropdown';
import PageButton from '../Components/pageButton';
import PostInput from '../Components/postInput';

class Forum extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            //Forum information recieved from server
            data: {
                forumTitle:{},
                comments: [],
                teamMembers: []
            },
            //How many forum posts to show on the page
            pageSize: 7,
            //The current page
            pageNum: 1,
            //The max amount of characters in a forum post
            maxCommentSize: 1000,
            //The current reply
            replyTo: 0,
            //Can go to previous page
            prevBtnDisabled: false,
            //Can go to next page
            nextBtnDisabled: false,
            //Can submit post
            submitDisabled: true,
            //Report to send
            reportText: "",
            //Files to be submitted with a post
            fileList: []
        };
    }

    componentDidMount = () => {
        //Gets the current page from the server after the component is done mounting
        this.getPage();
    }

    getPage = () => {
        let getData = {
            //Get the current forum to get comments for
            forumId: this.props.params.id,
            //The amount of posts to get
            pageSize: this.state.pageSize,
            //The page to get
            pageNum: this.state.pageNum
        }
        axios.post("/api/forum/comments/get", getData)
        .then(resp => {
            let data = resp.data.item;
            //Reverse comments to put newest on bottom of page
            data.comments = data.comments.reverse();
            //Calculate the max page
            let maxPage = Math.ceil(data.totalComments / this.state.pageSize);
            this.setState({
                data: data,
                maxPage: maxPage,
                prevBtnDisabled: currPage === maxPage,
                nextBtnDisabled: currPage === 1
            });
        });
    }

    handleReplyClick = id => {
        const comments = this.state.data.comments;
        //Find the index of the comment we clicked reply on
        let commentIdx = comments.findIndex(com =>{
            if(com.id === id) {
                return true;
            }
        });

        //If the reply wasn't found then return
        if(commentIdx === -1) {
            return;
        }

        //Get the reply data from the one we clicked
        let reply = comments[commentIdx];
        let replyText = reply.text;
        if(replyText.length > 15) {
            //Cut the reply down to 15 characters and add ellipsis
            replyText = replyText.slice(0, 15) + "...";
        }

        this.setState({
            replyTo: {
                id: id,
                name: reply.person.firstName + " " + reply.person.lastName,
                text: replyText,
                time: reply.createdDate
            }
        }, () => {
            //Scroll down to the input box
            $("html, body").animate({ scrollTop: $(document).height() });
        });
    }

    handlePageClick = page => {
        //If the user clicked on a disabled button don't do anything
        if((page === 'prev' && this.state.prevBtnDisabled) || (page === 'next' && this.state.nextBtnDisabled)){
            return;
        }
        
        //Calculate which direction to go based on the button the user clicked
        let currPage = this.state.pageNum;
        if(page === 'prev') {
            if(currPage < this.state.maxPage) {
                currPage++;
            }
        } else if(page === 'next') {
            if(currPage > 1) {
                currPage--;
            }
        }
        
        this.setState({
            pageNum: currPage
        }, () => this.getPage()) //Get the next page of results
    }

    handleInputChange = e => {
        let value = e.target.value;
        let submitDisabled = true;
        if(value.trim().length > 0) {
            //Enable the submit button if the user has typed anything other than whitespace
            submitDisabled = false;
        }
        //If the current comment is longer than the max comment size don't let them type anymore
        if(value.length > this.state.maxCommentSize) {
            //Slice the comment so user can paste text longer than 1000 characters but cuts off at 1000
            value = value.slice(0, this.state.maxCommentSize);
        }

        this.setState({
            [e.target.name]: value,
            submitDisabled: submitDisabled,
        });
    }

    handleSubmitClick = () => {
        const postComment = this.state.postComment;
        const fileList = this.state.fileList.map(file => {
            return file.id
        });
        
        let postData = {
            forumId: this.props.params.id,
            quoteId: this.state.replyTo.id,
            text: postComment,
            files: fileList
        }
        //Sends the current forum, the quote id, the post text, and a list of uploaded files to the server
        axios.post('/api/forum/comments', postData)
        .then(resp => {
            this.getPage(); //Update the page after a successful post
        })
        //Reset the form values
        this.setState({
            replyTo: 0,
            postComment: "",
            submitDisabled: true,
            fileList: []
        })
    }

    handleCancelReply = () => {
        //Clears the reply if the user cancels
        this.setState({
            replyTo: 0
        });
    }

    handleReportClick = id => {
        //Shows a modal so the user can submit a report
        $('#commentReport').modal('show');
        this.setState({
            reportedComment: id
        })
    }

    handleFileInput = e => {
        const files = e.target.files; 
        for(let i = 0; i < files.length ; i++){
            var formdata = new FormData();
            //Creates an object to upload a file to the server
            formdata.append("file", files[i]);
            //Sends the file to the server
            axios.post("/api/file/upload/0",formdata)
                .then(resp => {
                    let fileId = resp.data.item;
                    //Gets the uploaded file info
                    this.getFileById(fileId)
                    .then(file => {
                        //adds the file info to the state to show on the forum comment
                        this.setState((prevState, props) => {
                            fileList: prevState.fileList.push(file)
                        });
                    });
                });
        }
    }

    handleDeleteFile = id => {
        //Deletes the file, can only be used before submitting the comment
        axios.delete("/api/file/delete/" + id)
            .then(resp => {
                //After deleting the file from the server, remove it from the list so its removed from the DOM
                let fileList = this.state.fileList;
                let filtered = fileList.filter((file) => id !== file.id);
                this.setState({
                    fileList: filtered
                });
            })
    }

    getFileById = id => {
        //Gets information about a file from it's id
        return axios.get('/api/file/' + id)
            .then(resp => {
                return resp.data.item;
            });
    }

    render() {
        const data = this.state.data;
        //Information about the current forum
        const forumName = data.name;
        const forumDescription = data.description;

        //Needed for PostInput component
        const submitDisabled = this.state.submitDisabled;
        const replyTo = this.state.replyTo;
        const postComment = this.state.postComment;
        const maxCommentSize = this.state.maxCommentSize;
        const files = this.state.fileList.map((file) => {
            return <ForumFile 
                    key={file.id}
                    {...file}
                    deleteFile={this.handleDeleteFile}
                    awaitingSubmission={true}
                    />
        });

        //Needed for PageButton component
        const prevBtnDisabled = this.state.prevBtnDisabled;
        const nextBtnDisabled = this.state.nextBtnDisabled;

        //Needed for CommentReport component
        const reportedComment = this.state.reportedComment;

        return (
            <div>
                <div className="row wrapper border-bottom white-bg page-heading">
                    <div className="col-md-2">
                        <Link to='/forumindex' className="btn btn-primary forum-back-btn"><span aria-hidden="true">&larr;</span> Back To Forums</Link>                                        
                    </div>
                    <div className="col-md-8">
                        <h2 className="text-center">{forumName}</h2>
                        <h4 className="text-center">{forumDescription}</h4>
                    </div>
                    <div className="col-md-2">
                        <MemberDropdown teamList={data.teamMembers} />
                    </div>
                </div>
                <div className="forum-content">
                    <CommentList 
                        comments={data.comments} 
                        onReplyClick={this.handleReplyClick} 
                        onReportClick={this.handleReportClick} 
                    />
                    <PostInput 
                        submitDisabled={submitDisabled} 
                        replyTo={replyTo} 
                        value={postComment} 
                        maxLength={maxCommentSize}
                        fileList={files}
                        cancelReply={this.handleCancelReply} 
                        onInputChange={this.handleInputChange} 
                        onSubmitClick={this.handleSubmitClick} 
                        onFileInput={this.handleFileInput}
                    />
                    <PageButton 
                        prevBtnDisabled={prevBtnDisabled} 
                        nextBtnDisabled={nextBtnDisabled} 
                        onPageClick={this.handlePageClick} 
                    />
                </div>
                <CommentReport 
                    reportComment={reportedComment}
                    modalId='commentReport' 
                />
            </div>
        );
    }
}

export default Forum;