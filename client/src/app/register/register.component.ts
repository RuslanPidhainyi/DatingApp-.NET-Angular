import {
  Component,
  EventEmitter,
  inject,
  input,
  Input,
  OnInit,
  output,
  Output,
} from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { JsonPipe, NgIf } from '@angular/common';
import { TextInputComponent } from '../_forms/text-input/text-input.component';
import { DatePickerComponent } from "../_forms/date-picker/date-picker.component";

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, JsonPipe, NgIf, TextInputComponent, DatePickerComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent implements OnInit {
  private accountService = inject(AccountService);
  private fb = inject(FormBuilder);
  private toaster = inject(ToastrService);

  // pierwszy sposob //
  //@Input() usersFromHomeComponent: any; //za pomocy dekorotywnego "input'u" i wlasciwosci "input'a", chcemy przekazac wlasciwosci ("users" w pliku "register.component.html") do komponentu podrzednego

  // drugi sposob - dziala z wersji 17.3^ //
  //usersFromHomeComponent= input.required<any>();

  /******************************************************************************************************** */

  // pierwszy sposob //
  //@Output() cancelRegister = new EventEmitter() //dekorator "@output()" czyli dekorator wyjsciowy - ktory bedzie cos robic i zaincjąwaliscmy EventEmitter - emiter zdarzen

  // drugi sposob - dziala z wersji 17.3^ //
  cancelRegister = output<boolean>();

  model: any = {};
  registerForm: FormGroup = new FormGroup({});
  maxDate = new Date();

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8),
      ]],
      confirmPassword: ['', [
        Validators.required,
        this.matchValues('password'),
      ]],
    });

    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () =>
        this.registerForm.controls['confirmPassword'].updateValueAndValidity(),
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value
        ? null
        : { isMatching: true };
    };
  }

  register() {
    console.log(this.registerForm.value);
    // this.accountService.register(this.model).subscribe({
    //   next: response => {
    //     console.log(response);
    //     this.cancel();
    //   },
    //   error: error => this.toaster.error(error.error),
    // })
  }

  cancel() {
    this.cancelRegister.emit(false); //kiedy klikamy na przycisk wewnetrz naszego komponentu podrzedniego emitujemy wartosc false
  }
}
