import { Component, OnInit, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Data } from '../models/data.model';

@Injectable({
    providedIn: 'root'
  })
export class DataService {
url = 'http://localhost:4200/assets/example.json';

  constructor(private http: HttpClient) { }

  getData() {
    return this.http.get<Data[]>(this.url);
  }

  ngOnInit(): void {
  }
}