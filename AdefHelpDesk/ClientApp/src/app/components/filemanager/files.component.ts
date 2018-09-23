import {
    Component, OnInit, OnDestroy, Input, Output,
    ViewContainerRef, EventEmitter, ViewChild, trigger
} from '@angular/core';
import {
    Router, ActivatedRoute
} from '@angular/router';
import { Subscription } from 'rxjs/Subscription';

import {
    FileUploadModule,
    DropdownModule,
    DataTableModule,
    TreeModule,
    TreeNode,
    SelectItem,
    SharedModule,
    DialogModule,
    InputTextModule,
    FileUpload
} from 'primeng/primeng';

import { HTMLDialogService } from '../services/internal/htmldialog.service';
import { DialogService } from '../services/internal/dialog.service';
import { FilesService } from '../services/web/files.service';
import { IDTONode } from '../classes/DTONode';
import { IDTOResponse } from '../classes/dtoResponse';

@Component({
    selector: 'files',
    templateUrl: './files.component.html'
})
export class FilesComponent implements OnInit {
    @ViewChild('fileInput') fileInput: FileUpload;
    errorMessage: string;
    fileList: TreeNode[];
    selectedNode: TreeNode;
    showCreateFolderPopup: boolean = false;
    NewFolderName: string = "";

    // Register the service
    constructor(
        private _FilesService: FilesService,
        private _dialogService: DialogService,
        private _htmldialogService: HTMLDialogService) { }

    ngOnInit(): void {
        this.getFilesAndFolders();
    }

    public getFilesAndFolders() {
        this.errorMessage = "";

        //Clear Filelist
        this.fileList = [];

        // Call the service -- to get Files
        this._FilesService.getFiles()
            .subscribe((files) => {
                // Show the Files in the Tree Control
                this.fileList = files.children;
            },
            error => {
                this.errorMessage = <any>error;
                this._dialogService.setMessage(this.errorMessage);
            });
    }

    public downloadItem() {
        if (this.selectedNode !== undefined) {
            // Create an IDTONode
            let DTONode: IDTONode = {
                data: this.selectedNode.data,
                label: this.selectedNode.label,
                expandedIcon: this.selectedNode.expandedIcon,
                collapsedIcon: this.selectedNode.collapsedIcon,
                children: [],
                parentId: 0
            }

            // Call the service
            this._FilesService.getFile(DTONode)
                .subscribe(IDTOResponse => {
                    if (IDTOResponse.isSuccess) {
                        this._htmldialogService.setMessage(IDTOResponse.message);
                    } else {
                        this._dialogService.setMessage(IDTOResponse.message);
                    }
                },
                error => {
                    this.errorMessage = <any>error;
                    this._htmldialogService.setMessage(this.errorMessage);
                });
        }
    }
    public filesSelected(event) {
        for (var i = 0; i < event.files.length; i++) {
            var selectedFile = event.files[i].name;

            if (!this.fileList.find(x => x.label == selectedFile)) {
                alert('Files can only be replaced. ' + selectedFile + ' cannot be uploaded.');
                // Remove the file 
                var colFiles = this.fileInput.files.filter(x => x.name !== selectedFile);
                this.fileInput.files = colFiles;
            }
        };
    }

    public onBeforeUploadHandler(event) {
        // called before the file(s) are uploaded
        // Send the currently selected folder in the Header
        event.formData.append("selectedFolder", "");
    }

    public onUploadHandler(event) {
        // Called after the file(s) are upladed
        // Refresh the files and folders
        this.getFilesAndFolders();
    }
}