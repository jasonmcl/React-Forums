import React from 'react';
import axios from 'axios';
import ForumMapper from '../components/forums/forumMapper';

class ForumIndex extends React.Component {
    constructor(props) {
        super(props);
    }
    
    componentDidMount = () => {
        //Calls the getForums function after the component mounts
        this.getForums();
    }

    getForums = () => {
        //Gets the active and complete forums for the currently logged in user
        axios.get('/api/forum/currentuser')
        .then(resp => {
            let list = resp.data.items;
            this.setState({
                fullForums: list
            });
        })
    }

    render() {
        return (
            <ForumMapper forumList={this.state.fullForums}/>
        );
    }
}

export default ForumIndex;