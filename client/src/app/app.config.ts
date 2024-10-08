import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import {provideAnimations} from '@angular/platform-browser/animations';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';
import { errorInterceptor } from './_interceptors/error.interceptor';
import { jwtInterceptor } from './_interceptors/jwt.interceptor';
import { NgxSpinner, NgxSpinnerModule } from 'ngx-spinner';
import { loadingInterceptor } from './_interceptors/loading.interceptor';
import { TimeagoModule } from 'ngx-timeago';
import { ModalModule } from 'ngx-bootstrap/modal';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),//router
    provideHttpClient(withInterceptors([errorInterceptor, jwtInterceptor, loadingInterceptor])), //wstrzykiwania zaleznosci (injection)
    provideAnimations(), //sluzy dla animacji
    provideToastr({
      positionClass: 'toast-bottom-right'
    }), //sluzy dla powiadomienia
    importProvidersFrom(NgxSpinnerModule, TimeagoModule.forRoot(), ModalModule.forRoot()),
  ]
};
