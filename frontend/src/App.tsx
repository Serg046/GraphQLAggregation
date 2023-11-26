import React from 'react';
import logo from './logo.svg';
import './App.css';
import {Bar} from '@assetmark/widget-components';

function App() {
  return (
      <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo"/>
          <div style={{width: 788, height: 536}}>
            <Bar
                axisName='quarter'
                dataX={Array.from({length: 4}, (_, i) => `Q${i + 1}`)}
                series={{
                  name: 'Test',
                  data: [580, 123, 980, 10],
                }}/>
          </div>
        </header>
      </div>
  );
}

export default App;
