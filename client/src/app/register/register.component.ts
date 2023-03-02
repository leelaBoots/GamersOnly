import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, UntypedFormBuilder, FormControl, UntypedFormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  //model: any = {}; // we're not using model property, because we use registerForm now.
  registerForm: UntypedFormGroup; // provided by angular forms, we will use it for Reactive forms
  maxDate: Date;
  validationErrors: string[] = []; // initialize array to empty array because we check the length for error messages

  // inject the accountService into this component
  constructor(private accountService: AccountService, private toastr: ToastrService, private fb: UntypedFormBuilder, private router: Router) { }

  ngOnInit(): void {
    this.initializeForm();
    // insure user cannot enter a date more than 18 years ago
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() -18);
  }

  initializeForm() {
    // using formBuilder here simplifies our code a bit instead of specifying formBuilder for each input
    this.registerForm = this.fb.group({
      gender: ['female'],
      userame: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]]
    })

    // all of this below is so that changing the password after confirming it will not stay validated
    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })
  }

  // create our own custom validator function
  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      // return null means validation passed, isMatching: true means we have validation error of type isMatching
      return control?.value === control?.parent?.controls[matchTo].value ? null : {isMatching: true}
    }
  }

  register() {
    // our register is a Form
    //this.accountService.register(this.model).subscribe(response => { // old way
    this.accountService.register(this.registerForm.value).subscribe(response => {

      // take us straight to the members page
      this.router.navigateByUrl('/members');
    }, error => {
      this.validationErrors = error;
      // this.toastr.error(error.error); // we dont need this because bad request will come back from interceptor
    })
  }

  cancel() {
    // what we want to emit when the cancel button is pressed
    this.cancelRegister.emit(false);
  }

}
