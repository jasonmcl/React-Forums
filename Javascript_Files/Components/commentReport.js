import React from 'react';
import swal from 'sweetalert';
import axios from 'axios';

class CommentReport extends React.Component{
    constructor(props){
        super(props);
        this.state = {
            reportText: "",
            submitDisabled: true
        }
    }

    handleChange = (e) => {
        let name = e.target.name;
        let value = e.target.value;
        let submitDisabled = true;
        if(value.trim().length > 0) {
            //Enables the submit if the user input anything other than white space
            submitDisabled = false;
        }
        this.setState({
            [name]: value,
            submitDisabled: submitDisabled
        })
    }

    handleSubmitClick = () => {
        let reportText = this.state.reportText;
        let reportData = {
            CommentId: this.props.reportComment,
            ReportText: reportText,
        }
        //Sends the report to the server, where the admin can then see it
        axios.post("/api/forum/comments/report", reportData)
        .then(resp => {
            //Hides the modal and resets the form data
            $('#commentReport').modal('hide');
            this.setState({
                reportText: "",
                submitDisabled: true
            })
            //Shows a success alert
            swal({
                icon: "success"
            });
        });
    }

    render() {
        return(
            <div className='modal fade' tabIndex='-1' role='dialog' id={this.props.modalId}>
                <div className='modal-dialog'>
                    <div className='modal-content'>
                        <div className='modal-header'>
                            <button type='button' className='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>
                            <h2>Report this comment?</h2>
                        </div>
                        <div className='modal-body'>
                            <form>
                                <label>Tell us why you are reporting this comment: </label>
                                <textarea value={this.state.reportText} name="reportText" onChange={this.handleChange} style={{resize: 'none'}} rows='5' type="text" className="form-control">
                                </textarea>
                            </form>
                        </div>
                        <div className='modal-footer'>
                            <div>
                                <div className='btn-group'>
                                    <button type='button' className='btn btn-default' data-dismiss='modal'>Cancel</button>
                                    <button disabled={this.state.submitDisabled} onClick={this.handleSubmitClick} type='button' className='btn btn-primary'>Submit</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

export default CommentReport;