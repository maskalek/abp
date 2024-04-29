import {
  ChangeDetectionStrategy,
  Component,
  Injector,
  Input,
  TrackByFunction,
  inject,
} from '@angular/core';
import { EntityAction, EntityActionList } from '../../models/entity-actions';
import { EXTENSIONS_ACTION_TYPE } from '../../tokens/extensions.token';
import { AbstractActionsComponent } from '../abstract-actions/abstract-actions.component';
import { NgbDropdownModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { LocalizationModule, PermissionDirective, PermissionService } from '@abp/ng.core';
import { EllipsisDirective } from '@abp/ng.theme.shared';
import { NgClass, NgTemplateOutlet } from '@angular/common';

@Component({
  exportAs: 'abpGridActions',
  standalone: true,
  imports: [
    NgbDropdownModule,
    EllipsisDirective,
    PermissionDirective,
    NgClass,
    LocalizationModule,
    NgTemplateOutlet,
    NgbTooltipModule,
  ],
  selector: 'abp-grid-actions',
  templateUrl: './grid-actions.component.html',
  providers: [
    {
      provide: EXTENSIONS_ACTION_TYPE,
      useValue: 'entityActions',
    },
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GridActionsComponent<R = any> extends AbstractActionsComponent<EntityActionList<R>> {
  @Input() icon = 'fa fa-cog';

  @Input() readonly index?: number;

  @Input() text = '';

  readonly trackByFn: TrackByFunction<EntityAction<R>> = (_, item) => item.text;
  public readonly permissionService = inject(PermissionService);

  constructor(injector: Injector) {
    super(injector);
  }

  hasAvailableActions(): boolean {
    return this.actionList.toArray().some(action => {
      if (!action) return false;

      const { permission, visible } = action;

      return this.permissionService.getGrantedPolicy(permission) && visible(this.data);
    });
  }
}
