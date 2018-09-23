/* Defines the Task entity */
export interface ITask {
    taskId?: number;
    portalId?: number;
    description?: string;
    status?: string;
    priority?: string;
    createdDate?: string;
    estimatedStart?: string;
    estimatedCompletion?: string;
    dueDate?: string;
    assignedRoleId?: string;
    assignedRoleName?: string;
    ticketPassword?: string;
    requesterUserId?: string;
    requesterName?: string;
    requesterEmail?: string;
    requesterPhone?: string;
    estimatedHours?: string;
    sendEmails?: boolean;
    selectedTreeNodes?: number[];
    colDTOTaskDetail?: ITaskDetail[];
}

export interface ITaskDetail {
    detailId?: number;
    detailType?: string;
    contentType?: string;
    insertDate?: string;
    userId?: string;
    userName?: string;
    description?: string;
    emailDescription?: string;
    startTime?: string;
    stopTime?: string;
    sendEmails?: boolean;
    colDTOAttachment?: IAttachment[];
}

export interface IAttachment {
    attachmentID?: number;
    attachmentPath?: string;
    fileName?: string;
    originalFileName?: string;
    userId?: string;
}