import React from 'react';

/*
    Props
        teamList
*/
function MemberDropdown(props) {
    let teamList = props.teamList.map((member, index) => {
        //If the member is a captain, give them a rocket icon
        let captainClass = member.isCaptain ? "fa fa-rocket" : "";
        return <li key={index} className="list-group-item"><i className={captainClass}></i> {member.firstName} {member.lastName}</li>
    })
    return(
        <div className="dropdown pull-right forum-user-btn">
            <button type="button" className="btn btn-info" data-toggle="dropdown" id="userDropdown" onMouseOver={()=>{$('#userDropdown').dropdown('toggle')}}>
                <i className="fa fa-user"></i> Users <span className='caret'></span>
            </button>
            <ul className="dropdown-menu list-group">
                {teamList}
            </ul>
        </div>
    );
}

export default MemberDropdown;