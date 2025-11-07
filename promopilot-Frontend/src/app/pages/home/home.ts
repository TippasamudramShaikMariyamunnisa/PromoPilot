import { Component, AfterViewInit, inject } from '@angular/core';
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { NavbarComponent } from '../../layout/navbar/navbar';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NavbarComponent, CommonModule, RouterModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class HomeComponent implements AfterViewInit {
  private platformId = inject(PLATFORM_ID);

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    const tiltElements = document.querySelectorAll('.pointer-shadow');
    tiltElements.forEach(el => {
      el.addEventListener('mousemove', event => {
        const mouseEvent = event as MouseEvent;
        const target = el as HTMLElement;
        const rect = target.getBoundingClientRect();
        const x = mouseEvent.clientX - rect.left;
        const y = mouseEvent.clientY - rect.top;
        const centerX = rect.width / 2;
        const centerY = rect.height / 2;
        const rotateX = ((y - centerY) / centerY) * 5;
        const rotateY = ((x - centerX) / centerX) * -5;
        target.style.transform = `rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
      });

      el.addEventListener('mouseleave', () => {
        const target = el as HTMLElement;
        target.style.transform = 'rotateX(0deg) rotateY(0deg)';
      });
    });
  }
}
