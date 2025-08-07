import React from 'react';
import { Button, Typography } from '@mui/material';
import logo from './logo.svg';
import './App.css';

function App() {
  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <Typography variant="h4" color="inherit">
          Edit <code>src/App.tsx</code> and save to reload.
        </Typography>
        <Button 
          variant="contained" 
          color="primary" 
          href="https://reactjs.org"
          target="_blank" 
          rel="noopener noreferrer"
        >
          Learn React
        </Button>
      </header>
    </div>
  );
}

export default App;
