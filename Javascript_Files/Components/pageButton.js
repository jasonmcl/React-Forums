import React from 'react';

/*
    Props
        prevBtnDisabled - disables the previous button
        nextBtnDisabled - disables the next button
        onPageClick - called when either button is clicked, sends 'prev' or 'next' as parameter
*/
function PageButton(props) {
    return (
        <nav>
            <ul className="pager forum-pager">
                <li className={"previous" + (props.prevBtnDisabled ? " disabled" : "")} onClick={() => props.onPageClick('prev')}><a><span aria-hidden="true">&larr;</span> Older</a></li>
                <li className={"next" + (props.nextBtnDisabled ? " disabled" : "")} onClick={() => props.onPageClick('next')}><a>Newer <span aria-hidden="true">&rarr;</span></a></li>
            </ul>
        </nav>
    );
}

export default PageButton;