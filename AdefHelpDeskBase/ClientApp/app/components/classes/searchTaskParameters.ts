export interface ISearchTaskParameters {
    id?: string;
    userId?: string;
    searchText?: string;
    status?: string;
    priority?: string;
    createdDate?: string;
    dueDate?: string;
    assignedRoleId?: string;
    selectedTreeNodes?: number[];
    sortOrder?: string;
    sortField?: string;
    rowsPerPage: number;
    pageNumber: number;
}