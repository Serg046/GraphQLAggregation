/* eslint-disable */
import { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
};

export type PassportType = {
  __typename?: 'PassportType';
  id: Scalars['Int']['output'];
  number: Scalars['String']['output'];
};

export type Query = {
  __typename?: 'Query';
  users?: Maybe<Array<Maybe<UserType>>>;
  users2?: Maybe<Array<Maybe<UserType2>>>;
  users3?: Maybe<Array<Maybe<UserType3>>>;
};

export type UserType = {
  __typename?: 'UserType';
  age: Scalars['Int']['output'];
  firstName: Scalars['String']['output'];
  kids: Scalars['Int']['output'];
  lastName: Scalars['String']['output'];
  passport?: Maybe<PassportType>;
};

export type UserType2 = {
  __typename?: 'UserType2';
  age: Scalars['Int']['output'];
  firstName: Scalars['String']['output'];
  kids: Scalars['Int']['output'];
  lastName: Scalars['String']['output'];
  passport?: Maybe<PassportType>;
};

export type UserType3 = {
  __typename?: 'UserType3';
  age?: Maybe<Scalars['Int']['output']>;
  firstName?: Maybe<Scalars['String']['output']>;
  kids?: Maybe<Scalars['Int']['output']>;
  lastName?: Maybe<Scalars['String']['output']>;
  passport?: Maybe<PassportType>;
};

export type AllKidsByLastNamesQueryVariables = Exact<{ [key: string]: never; }>;


export type AllKidsByLastNamesQuery = { __typename?: 'Query', users?: Array<{ __typename?: 'UserType', lastName: string, kids: number } | null> | null };


export const AllKidsByLastNamesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"allKidsByLastNames"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"users"},"directives":[{"kind":"Directive","name":{"kind":"Name","value":"aggregation"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"by"},"value":{"kind":"StringValue","value":"lastName","block":false}}]}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"kids"},"directives":[{"kind":"Directive","name":{"kind":"Name","value":"sum"}}]}]}}]}}]} as unknown as DocumentNode<AllKidsByLastNamesQuery, AllKidsByLastNamesQueryVariables>;