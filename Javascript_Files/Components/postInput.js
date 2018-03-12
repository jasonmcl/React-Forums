import React from 'react';

/*
    Props
        value
        replyTo
        maxLength
        fileList
        submitDisabled
        cancelReply - function cancels the reply
        onSubmitClick - function submits the forum post
        onFileInput - function adds files to the comment
        onInputChange - function called when comment form changes
*/
function PostInput(props) {
    let length;
    if(!props.value) {
        length = 0;
    }
    else {
        length = props.value.length;
    }
    return (
        <div className="panel panel-default">
            <div className="panel-body">
                {props.replyTo !== 0 && 
                    <div>
                        <h3>Replying to: {props.replyTo.name} - {props.replyTo.text} - {props.replyTo.time} <span className="pull-right forum-links"><a onClick={props.cancelReply}>Cancel</a></span></h3>
                    </div>
                }
                <textarea style={{resize: 'none'}} name="postComment" onChange={props.onInputChange} className="form-control forum-comment-input" rows='5' value={props.value}>
                </textarea>
                <small>{length} / {props.maxLength}</small>
                <br/>
                <div className="col-md-10">
                    {props.fileList}
                </div>
                <div className="col-md-2">
                    <div className="row">
                        <input onChange={props.onFileInput} type="file" id="fileUpload" className="inputfile inputfile-2" multiple/>
                        <label htmlFor="fileUpload" className='text-center'><svg xmlns="http://www.w3.org/2000/svg" width="20" height="17" viewBox="0 0 20 17"><path d="M10 0l-5.2 4.9h3.3v5.1h3.8v-5.1h3.3l-5.2-4.9zm9.3 11.5l-3.2-2.1h-2l3.4 2.6h-3.5c-.1 0-.2.1-.2.1l-.8 2.3h-6l-.8-2.2c-.1-.1-.1-.2-.2-.2h-3.6l3.4-2.6h-2l-3.2 2.1c-.4.3-.7 1-.6 1.5l.6 3.1c.1.5.7.9 1.2.9h16.3c.6 0 1.1-.4 1.3-.9l.6-3.1c.1-.5-.2-1.2-.7-1.5z"/></svg> <span>Upload Files&hellip;</span></label>
                    </div>
                    <div className="row">
                        <button onClick={props.onSubmitClick} disabled={props.submitDisabled} className="btn btn-primary btn-block">Submit</button>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default PostInput;