export interface ICategory {
    categoryId: number;
    label?: string;
    expandedIcon?: any;
    collapsedIcon?: any;
    children?: ICategory[];
    parentId?: number;
    parent?: ICategory;
    type: string;
    selectable: boolean;
    data?: INodeDetail; 
}

export interface INodeDetail {
    categoryId?: string;
    checkboxChecked?: boolean;
    selectable: boolean;
    requestorVisible: boolean;
}