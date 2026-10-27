import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ControlVolumenComponent } from './control-volumen.component';

describe('ControlVolumenComponent', () => {
  let component: ControlVolumenComponent;
  let fixture: ComponentFixture<ControlVolumenComponent>;

  beforeEach(() => {
    fixture = TestBed.createComponent(ControlVolumenComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
