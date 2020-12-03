import { Component } from '@angular/core';
import { ChartsModule, WavesModule } from 'angular-bootstrap-md'
import {BaseChartDirective} from 'angular-bootstrap-md'
import { Chart } from 'node_modules/chart.js/'
import { DataService } from '../../services/data.service';
import { Data } from '../../models/data.model';

@Component({
  selector: 'app-results',
  templateUrl: './results.component.html',
  styleUrls: ['./results.component.css']
})
export class ResultsComponent {

//JSON to Array Sample
//let jsonObject;
//let values : string array;

//jsonObject.foreach(element => {
//    values.push(element.name);
//})

  
  // Services
  data$: Data[];

  constructor(private dataService: DataService) {}
  
  ngOnInit() {
      return this.dataService.getData()
       .subscribe(data => this.data$ = data);
    }

  humidity: Array<number> = [65, 59, 80, 81, 56, 55, 40];
  vibration: Array<number> = [28, 48, 40, 19, 86, 27, 90];
  time: Array<string> = ['12:00', '12:05', '12:10', '12:15', '12:20', '12:25', '12:30'];

  public chartType: string = 'line';

  public chartDatasets: Array<any> = [
    { data: this.humidity, label: 'Feuchtigkeit' },
    { data: this.vibration, label: 'Vibration' }
  ];

  public chartLabels: Array<any> = this.time;

  public chartColors: Array<any> = [
    {
      backgroundColor: 'rgba(105, 0, 132, .2)',
      borderColor: 'rgba(200, 99, 132, .7)',
      borderWidth: 2,
    },
    {
      backgroundColor: 'rgba(0, 137, 132, .2)',
      borderColor: 'rgba(0, 10, 130, .7)',
      borderWidth: 2,
    }
  ];

  public chartOptions: any = {
    responsive: true,
    lineTension: 0,
    bezierCurve: false
  };
  public chartClicked(e: any): void { }
  public chartHovered(e: any): void { }

}
