/* Defines the DTONode entity */
export interface IDTONode {
    data: string;
    label: string;
    expandedIcon: string;
    collapsedIcon: string;
    children: IDTONode[];
    parentId: number;
}