import React from 'react';
import {FormattedDate, FormattedTime} from 'react-intl';
import ForumFile from './forumFile';

/*
    Props
        firstName
        lastName
        text
        createdDate
*/
function Quote(props) {
    let userName = props.firstName + ' ' + props.lastName;
    let text = props.text;
    let date = <FormattedDate value={props.createdDate}/>;
    let time = <FormattedTime value={props.createdDate}/>;
    return (
        <div className="well">
            <i className="fa fa-quote-left fa-3x fa-pull-left fa-border" aria-hidden="true"></i>
            <br/>
            <p>{text}</p>
            <footer className="text-right">{userName} - {date} - {time}</footer>
        </div>
    );
}

/*
    Props
        id
        text
        person
            firstName
            lastName
            role
            isCaptain
            profilePic
        uploadedFiles - list of files attached to a comment
        children - the quoted comment
        onReplyClick - function when reply is clicked
        onReportClick - function when report is clicked
*/
function CommentPanel(props) {
    let userName = props.person.firstName + ' ' + props.person.lastName;
    let text = props.text;
    let userRole = props.person.role;
    //Checks if the student is the team captain or just a team member
    if(userRole === 'Student') {
        userRole = 'Team ' + (props.person.isCaptain ? "Leader" : "Member");
    }
    let userPic = props.person.profilePic;
    let postDate = <FormattedDate
                        value={props.createdDate}
                        year='numeric'
                        month='long'
                        day='2-digit'
                    />;
    let postTime = <FormattedTime value={props.createdDate} />;
    //Creates the list of files attached to this forum post
    let fileList = props.uploadedFiles.map(file => {
        return <ForumFile
                key={file.id}
                    {...file}
                awaitingSubmission={false}
        />
    })
    
    return (
        <div className="panel panel-default" id={'comment' + props.id}>
            <div className="panel-body forum-panel">
                <div className="forum-flex">
                    <div className="forumSide">
                        <a className="forum-avatar">
                            <img src={userPic} className="img-circle" alt="image" />
                            <div className="author-info">
                                <p><strong>{userName}</strong></p>
                                <p><strong>{userRole}</strong></p>
                            </div>
                        </a>
                    </div>
                    <div className="forum-body">
                        <p>Posted: {postDate} - {postTime}</p>
                        {props.children}
                        <br/>
                        <p>{text}</p>
                        <br/>
                        {fileList}
                        <ul className="list-inline pull-right forum-links">
                            <li><a onClick={() => props.onReplyClick(props.id)}>Reply</a></li>
                            <li><a onClick={() => props.onReportClick(props.id)}>Report</a></li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    );
}

/*
    Props
        comments
        onReplyClick - function when reply is clicked
        onReportClick - function when report is clicked
*/
function CommentList(props) {
    const commentList = props.comments.map(elem => {
        return (
            <CommentPanel
                key={elem.id}
                {...elem}
                onReplyClick={props.onReplyClick}
                onReportClick={props.onReportClick}
            >
                {elem.quote &&
                    <Quote {...elem.quote} />
                }
            </CommentPanel>
        );
    });
    return(
        <div>
            {commentList}
        </div>
    );
}

export default CommentList;