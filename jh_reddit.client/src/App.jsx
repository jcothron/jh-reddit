import { useEffect, useState, useCallback } from 'react';
import { Table, TableBody, TableCell, TableContainer,
    TableHead, TableRow, Paper, FormControl, Select,
    InputLabel, MenuItem, Grid, Button} from '@mui/material';

import PropTypes from 'prop-types';
import settings from './settings.json';
import './App.css';

function Posts({ posts }) {
    return (
        <TableContainer component={Paper}>
            <Table sx={{ minWidth: 650 }} aria-label="simple table">
                <TableHead>
                    <TableRow>
                        <TableCell>Title</TableCell>
                        <TableCell align="center">Author</TableCell>
                        <TableCell align="center">Beginning UpVotes</TableCell>
                        <TableCell align="center">Current UpVotes</TableCell>
                        <TableCell align="center">UpVotes While Tracking</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {posts.map((post) => (
                        <TableRow key={post.id} sx={{ '&:last-child td, &:last-child th': { border: 0 } }}>
                            <TableCell component="th" scope="post">{post.title}</TableCell>
                            <TableCell>{post.author}</TableCell>
                            <TableCell>{post.beginningUps}</TableCell>
                            <TableCell>{post.currentUps}</TableCell>
                            <TableCell>{post.upVotes}</TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
}

Posts.propTypes = {
    posts: PropTypes.array
}


function App() {
    const [subReddit, setSubReddit] = useState({ name: "apple", limits: {}, posts: [], topAuthor: "", topPost: {} });
    const [isRunning, setIsRunning] = useState(false);
    const [message, setMessage] = useState("");

    const fetchData = useCallback(() => {
        setMessage("");
        var requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(subReddit)
        };
        fetch('./api/RedditData/SubReddit', requestOptions)
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                else {
                    console.log(response);
                    var message = response.status == "401" ? "Not Authorized to use the RedditAPI, please check appsettings for authorized bearer token."
                        : "There was an error in execution, please try again. If the problem persists, please contact an administrator.";
                    throw new Error(message);
                }
            })
            .then(data => {
                setSubReddit(data);
            }).catch(error => {
                setIsRunning(false);
                setMessage(error.message);
            });
    }, [subReddit]);


    useEffect(() => {
        if (isRunning) {
            //set to ten just to make sure limits aren't reached
            if (subReddit.limits.remaining > 10) {
                fetchData();
            } else {
                var pauseTime = subReddit.limits.reset * 1000;
                setMessage("Reddit Api Limits reached, execution paused for " + pauseTime + "seconds.")
                setTimeout(pauseTime, fetchData());
            }
        }
    }, [subReddit, isRunning, fetchData]);

    const onSelectChange = event => {
        var obj = { ...subReddit };
        obj.name = event.target.value;
            setSubReddit(obj);
    }

    return (
            <Grid container spacing={2}>
                <Grid item xs={6} >
                <FormControl fullWidth>
                        <InputLabel id="subReddit-select-label">subReddit</InputLabel>
                        <Select labelId="subReddit-select-label" id="subReddit-select" value={subReddit.name}
                            label="subReddit" disabled={isRunning} onChange={onSelectChange}>
                        {settings.subReddits.map(sub =>
                            <MenuItem key={sub.id} value={sub.id}>{sub.name}</MenuItem>     
                        )}
                  
                        </Select>
                    </FormControl>
                </Grid>
                <Grid item xs={3}><Button disabled={isRunning} onClick={() => setIsRunning(true)}>Track</Button></Grid>
                <Grid item xs={3}><Button disabled={!isRunning} onClick={() => setIsRunning(false)}>Stop</Button></Grid>
                <Grid item xs={6}><InputLabel>Top Author: </InputLabel></Grid>
                <Grid item xs={6}><InputLabel>Top Post: </InputLabel></Grid>
                <Grid item xs={6}><InputLabel>{subReddit.topAuthor}</InputLabel></Grid>
                <Grid item xs={6}><InputLabel>{subReddit.topPost.title}</InputLabel></Grid>
                <Grid item xs={12}><FormControl fullWidth><InputLabel>{message}</InputLabel></FormControl></Grid>
                <Grid item xs={12}><Posts posts={subReddit.posts} /></Grid>
            </Grid>
    );
}

export default App;