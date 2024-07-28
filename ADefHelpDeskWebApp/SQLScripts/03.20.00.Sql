-- =============================================
-- Author:		Michael Washington
-- Create date: 2/2018
-- Description:	Searches ADefHelpDesk_Tasks
-- =============================================
ALTER Procedure [dbo].[spSearchTasks]
(
	@paramIsAdmin [int],
	@paramUserId [int],
	@paramSearchText [nvarchar] (1000),
	@paramStatus [nvarchar] (100),
	@paramPriority [nvarchar] (100),
	@paramCreatedDate [nvarchar] (100),
	@paramDueDate [nvarchar] (100),
	@paramAssignedRoleId [int],
	@paramSelectedTreeNodes [nvarchar] (MAX),
	@paramSortOrder [nvarchar] (100),
	@paramSortField [nvarchar] (100),
	@paramRowsPerPage [int],
	@paramPageNumber [int],
	@paramTotalCount [int] OutPut
)

AS
BEGIN
-- =============================================
-- Declare variables
-- =============================================
DECLARE @sql nvarchar(MAX)
DECLARE @sqlWhereClause nvarchar(MAX)
DECLARE @sqlOrderByWhereClause nvarchar(MAX)
DECLARE @IsAdmin [int];
DECLARE @UserId [int];
DECLARE @SearchText [nvarchar] (1000);
DECLARE @Status [nvarchar] (100);
DECLARE @Priority [nvarchar] (100);
DECLARE @CreatedDate [nvarchar] (100);
DECLARE @DueDate [nvarchar] (100);
DECLARE @AssignedRoleId [int];
DECLARE @SelectedTreeNodes [nvarchar] (MAX);
DECLARE @SortOrder [nvarchar] (100);
DECLARE @SortField [nvarchar] (100);
DECLARE @RowsPerPage [int];
DECLARE @PageNumber [int];

-- =============================================
-- Set variables
-- =============================================
SET @IsAdmin = @paramIsAdmin;
SET @UserId = @paramUserId;
SET @SearchText = @paramSearchText;
SET @Status = @paramStatus;
SET @Priority = @paramPriority;
SET @CreatedDate = @paramCreatedDate;
SET @DueDate = @paramDueDate;
SET @AssignedRoleId = @paramAssignedRoleId;
SET @SelectedTreeNodes = @paramSelectedTreeNodes;
SET @SortOrder = @paramSortOrder;
SET @SortField = @paramSortField;
SET @RowsPerPage = @paramRowsPerPage;
SET @PageNumber = @paramPageNumber;

-- *******************************************************************
-- Cleanup
-- *******************************************************************
if(@SortField = 'assignedRoleName')
BEGIN
	SET @SortField = 'AssignedRoleID'
END

-- =============================================
-- Query
-- =============================================

/*
Where Clause
*/
SELECT @sqlWhereClause = 'where (ADefHelpDesk_Tasks.Description like ''%'' + @SearchText + ''%'''
SELECT @sqlWhereClause = @sqlWhereClause + ' or (cast(ADefHelpDesk_Tasks.TaskID as nvarchar) = @SearchText)) '

/*
If not an Administrator you can only get your own tickets
*/
IF (@IsAdmin = 0)
BEGIN
	SELECT @sqlWhereClause = @sqlWhereClause + 'AND (ADefHelpDesk_Tasks.RequesterUserID = ' + cast(@UserId as nvarchar) + ') '
END

/*
Tree nodes
*/
IF NOT (@SelectedTreeNodes = '')
BEGIN
	SELECT @sqlWhereClause = @sqlWhereClause + 'AND (ADefHelpDesk_TaskCategories.CategoryID IN (' + @SelectedTreeNodes + ')) '
END

/*
Status
*/
IF NOT (@Status = '') AND NOT (@Status = 'ALL')
BEGIN
	SELECT @sqlWhereClause = @sqlWhereClause + 'AND (ADefHelpDesk_Tasks.Status = ''' + @Status + ''') '
END

/*
Priority
*/
IF NOT (@Priority = '') AND NOT (@Priority = 'ALL')
BEGIN
	SELECT @sqlWhereClause = @sqlWhereClause + 'AND (ADefHelpDesk_Tasks.Priority = ''' + @Priority + ''') '
END

/*
AssignedRoleId
*/
IF NOT (@AssignedRoleId = 0) 
BEGIN
	SELECT @sqlWhereClause = @sqlWhereClause + 'AND (ADefHelpDesk_Tasks.AssignedRoleID = ' + cast(@AssignedRoleId as nvarchar) + ') '
END

/*
Created Date
*/
IF NOT (@CreatedDate = '') 
BEGIN
	SELECT @sqlWhereClause = @sqlWhereClause + 'AND (ADefHelpDesk_Tasks.CreatedDate >= CONVERT(DATETIME, ''' + @CreatedDate + ''', 102)) '
END

/*
Due Date
*/
IF NOT (@DueDate = '') 
BEGIN
	SELECT @sqlWhereClause = @sqlWhereClause + 'AND (ADefHelpDesk_Tasks.DueDate >= CONVERT(DATETIME, ''' + @DueDate + ''', 102)) '
END

/*
*******************************************************************
Order By
*******************************************************************
*/
IF NOT (@SortField = '')
BEGIN
	SELECT @sqlOrderByWhereClause = 'ORDER BY ' + @SortField + ' '
END

IF (@SortField = '')
BEGIN
	SELECT @sqlOrderByWhereClause = 'ORDER BY ADefHelpDesk_Tasks.TaskID '
END

IF NOT (@SortOrder = '')
BEGIN
	SELECT @sqlOrderByWhereClause = @sqlOrderByWhereClause + @SortOrder + ' '
END

/*
*******************************************************************
Main Query
*******************************************************************
*/
SELECT @sql = 'Select DISTINCT ADefHelpDesk_Tasks.* from ADefHelpDesk_Tasks ' 
SELECT @sql = @sql + 'LEFT OUTER JOIN ADefHelpDesk_TaskCategories ON ' 
SELECT @sql = @sql + 'ADefHelpDesk_Tasks.TaskID = ADefHelpDesk_TaskCategories.TaskID ' 
SELECT @sql = @sql + @sqlWhereClause
SELECT @sql = @sql + @sqlOrderByWhereClause

/*
*******************************************************************
Paging
*******************************************************************
*/
SELECT @sql = @sql + 'OFFSET @RowsPerPage * (@PageNumber - 1) ROWS '
SELECT @sql = @sql + 'FETCH NEXT @RowsPerPage ROWS ONLY; '

/*
*******************************************************************
Get the record count
*******************************************************************
*/
SELECT @sql = @sql + 'Select @cnt = count(DISTINCT ADefHelpDesk_Tasks.TaskID) from ADefHelpDesk_Tasks '
SELECT @sql = @sql + 'LEFT OUTER JOIN ADefHelpDesk_TaskCategories ON ' 
SELECT @sql = @sql + 'ADefHelpDesk_Tasks.TaskID = ADefHelpDesk_TaskCategories.TaskID ' 
SELECT @sql = @sql + @sqlWhereClause
SELECT @sql = @sql + '; '

EXEC sp_executesql @sql, 
N'
@SearchText [nvarchar] (1000), 
@RowsPerPage [int], 
@PageNumber [int],
@SortField [nvarchar] (100),
@SortOrder [nvarchar] (100),
@UserId [int],
@SelectedTreeNodes [nvarchar] (MAX),
@Status [nvarchar] (100),
@Priority [nvarchar] (100),
@AssignedRoleId [int],
@CreatedDate [nvarchar] (100),
@DueDate [nvarchar] (100),
@cnt [int] OUTPUT
', 
@SearchText, 
@RowsPerPage, 
@PageNumber,
@SortField,
@SortOrder,
@UserId,
@SelectedTreeNodes,
@Status,
@Priority,
@AssignedRoleId,
@CreatedDate,
@DueDate,
@cnt = @paramTotalCount OUTPUT

END