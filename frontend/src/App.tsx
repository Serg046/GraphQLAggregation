import React from 'react';
import logo from './logo.svg';
import './App.css';
import {Bar} from '@assetmark/widget-components';
import {graphql} from "./gql";
import {useQuery} from '@apollo/client'

const allKidsByLastNames = graphql(/* GraphQL */ `
    query allKidsByLastNames {
        users @aggregation(by: "lastName")
        {
            lastName
            kids @sum
        }
    }
`)

function App() {
  const {data} = useQuery(allKidsByLastNames)

  let x = data?.users?.map(u => u?.lastName ?? '') ?? [];
  let y = data?.users?.map(u => u?.kids ?? 0) ?? [];
  return (
      <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo"/>
          <div style={{width: 788, height: 536}}>
            <Bar
                axisName='Names'
                dataX={x}
                series={{
                  name: '# of Kids',
                  data: y,
                }}/>
          </div>
        </header>
      </div>
  );
}

export default App;
