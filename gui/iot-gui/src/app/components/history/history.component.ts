import { Component, OnInit } from '@angular/core';
import  *  as  data  from  '../../../assets/json/example.json';


@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.css']
})
export class HistoryComponent implements OnInit {

  

  constructor() { }

  ngOnInit(): void {
    console.log(data);
  }

}
