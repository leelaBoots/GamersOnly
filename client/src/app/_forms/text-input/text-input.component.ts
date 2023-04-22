import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})

// ControlValueAccessor is a bridge beween Angular Forms API and a native element in the DOM
export class TextInputComponent implements ControlValueAccessor {
  // added some input properties here:
  @Input() label : string;
  @Input() type = 'text';

  // @Self decorator tells angular to keep this self contained and always inject this code here locally into this component
  constructor(@Self() public ngControl: NgControl) {
    // this gives us access to our control inside this component when we use it 
    this.ngControl.valueAccessor = this;
  }

  // the methods will pass thru to the interfaces default methods
  writeValue(obj: any): void {
  }
  registerOnChange(fn: any): void {
  }
  registerOnTouched(fn: any): void {
  }

}
