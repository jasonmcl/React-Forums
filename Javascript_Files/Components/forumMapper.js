import React from 'react';
import {Link} from 'react-router';
import {FormattedDate, FormattedTime} from 'react-intl';

/*
    Props
        title - active or completed
        children - List of forum posts
*/
function ForumCollection(props) {
    return (
        <div className="row">
            <div className="col-lg-12">
                <div className="wrapper wrapper-content animated fadeInRight">
                    <div className="ibox-content forum-container">
                        <div className="forum-title">
                            <h3>{props.title}</h3>
                        </div>
                        {props.children}
                    </div>
                </div>
            </div>
        </div>
    );
}

/*
    Props
        name
        description
        totalComments
        isActive
        modifiedDate
*/
function ForumPost(props) {
    const Title = props.name;
    const Desc = props.description;
    const Posts = props.totalComments;
    const IsActive = "forum-item" + (props.isActive? " active": "");
    return (
        <div className={IsActive}>
            <div className="row">
                <div className="col-md-9">
                    <div className="forum-icon">
                        <i className="fa fa-star"></i>
                    </div>
                    <Link to={'/forum/' + props.id} className="forum-item-title">{Title}</Link>
                    <div className="forum-sub-title">{Desc}</div>
                </div>
                <div className="col-md-1 forum-info pull-right">
                    <span className="views-number">
                        {Posts}
                    </span>
                    <div>
                        <small>Posts</small>
                    </div>
                </div>
                <div className="col-md-1 forum-info pull-right">
                    <h5>Last Modified:</h5>
                    <h5><FormattedDate value={props.modifiedDate}/></h5>
                    <h5><FormattedTime value={props.modifiedDate}/></h5>
                </div>
            </div>
        </div>
    );
}

function NoForums(props) {
    return (
        <div className="row">
            <div className="col-lg-12">
                <div className="wrapper wrapper-content animated fadeInRight">
                    <div className="ibox-content forum-container">
                        <div className="forum-title text-center">
                            <h3>You don't have any forums yet! Go make a project!</h3>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

/*
    Props
        forumList
*/
function ForumMapper(props) {
    //If the forum list doesn't exist, return an empty div
    if(!props.forumList){
        return <div></div>;
    }
    
    //Create a list of forum collections with a list of forum posts inside
    let list = props.forumList.map(status => {
        if(status.forums.length > 0) {
            return (
                <ForumCollection key={status.description} title={status.description}>
                {
                    status.forums.map(forum =>
                        <ForumPost key={forum.id} {...forum} isActive={status.description === 'Active'}/>
                    )
                }
                </ForumCollection>
            );
        }
    });

    //Checks if the user has any forums to show
    let hasForum = false;
    for(let i = 0; i < list.length; i++) {
        if(list[i] !== undefined){
            hasForum = true;
            break;
        }
    }
    return(
        <div>
            {hasForum ? list : <NoForums />}
        </div>
    );
}

export default ForumMapper;