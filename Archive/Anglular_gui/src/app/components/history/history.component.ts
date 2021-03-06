import { Component, ViewChild, OnInit, /**AfterViewInit,*/ ChangeDetectorRef } from '@angular/core';
import { MdbTableDirective, MdbTablePaginationComponent } from 'angular-bootstrap-md';
import { Data } from '../../models/data.model';
import { DataService } from '../../services/data.service';
import { LogService } from '../../log/log.service';


@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.css'],
})

export class HistoryComponent implements OnInit/**, AfterViewInit*/ {

  @ViewChild(MdbTablePaginationComponent, { static: true }) mdbTablePagination: MdbTablePaginationComponent;
  @ViewChild(MdbTableDirective, { static: true }) mdbTable: MdbTableDirective;
  headElements = ['Time', 'Date', 'Feuchtigkeit', 'Vibration 1', 'Vibration 2'];
  
  // Services
  data$: Data[];

  constructor(private dataService: DataService, private logger: LogService, /**private cdRef: ChangeDetectorRef*/) {}
  
  ngOnInit() {
      return this.dataService.getData()
       .subscribe(data => this.data$ = data);
       
    }
  
    log() {
      return this.logger.log(this.data$);
    }
   
}
