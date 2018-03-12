import React from 'react';
import {FormattedDate, FormattedTime} from 'react-intl';
let filesize = require('file-size');

/*
    Props
        size
        type
        systemFileName
        fileType
        id
        fileName
        createdDate
        awaitingSubmission - decides whether to show the delete button or not
        deleteFile - function called when delete button clicked
*/
const ForumFile = (props) => {
   const fileSize = filesize(props.size).human();
   let media;
   //Switches how to show the file based on the file type
   switch(props.type){
       case 'video/webm':
       case 'video/mkv':
       case 'video/flv':
       case 'video/avi':
       case 'video/wmv':
       case 'video/mp4':
            media = (
                <video width="370" height="214">
                    <source src={props.systemFileName} /> 
                    Your browser does not support the video tag.
                </video>
            )
            break;
       case 'image/jpeg':
       case 'image/png':
            media = (
                <div className="image">
                    <img alt="image" className="img-responsive" src={props.systemFileName}/>
                </div>
            )
            break;
       default:
            media = (
                <div className="icon">
                    <i className="fa fa-file"></i>                    
                </div>
            )
   }
   if(props.fileType === 'application/vnd.openxmlformats-officedocument.word'){
    media = (<div className="icon">
              <i className="fa fa-file-word-o"></i>
             </div>)
   }
   return (
        <div className="file-box" data-file-id={props.id}>
            <div className="file">
                <a href={props.systemFileName} target="_blank">
                    <span className="corner"></span>
                    {media}
                    <div className="file-name">
                        {props.fileName}
                        <br/>
                        <small>Added: <FormattedDate
                                        value={props.createdDate}
                                        day="numeric"
                                        month="long"
                                        year="numeric" /></small>
                        <br/>
                        <small>Size: {fileSize}</small>
                    </div>
                </a>
                {/* Show the delete button if the comment has not been submitted */}
                {props.awaitingSubmission && 
                    <p className='trash-icon'><i className="fa fa-trash" onClick={() => props.deleteFile(props.id)}></i></p>
                }
            </div>
        </div>
    )
}

export default ForumFile;