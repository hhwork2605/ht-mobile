import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { definePreset } from '@primeng/themes';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth.interceptor';

// Preset PrimeNG khớp brand ShopDunk Admin (#0070F4).
const HtPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#E5F1FE', 100: '#CCE2FD', 200: '#99C5FB', 300: '#66A9F9', 400: '#338DF6',
      500: '#0070F4', 600: '#005AC3', 700: '#005AC3', 800: '#004492', 900: '#002E61', 950: '#001731',
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimationsAsync(),
    providePrimeNG({ theme: { preset: HtPreset, options: { darkModeSelector: '.ht-dark-never' } } }),
  ],
};
