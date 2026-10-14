using Dapper;
using HRIStwinbeeAPI.DTO.Inventory.ItemMaster;
using HRIStwinbeeAPI.Repo.Interface.ItemMaster;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HRIStwinbeeAPI.Repo.ItemMaster
{
    public class ItemMasterRepository : IItemMasterRepository
    {
        private readonly IConfiguration _configuration;

        public ItemMasterRepository(
            IConfiguration configuration
        )
        {
            _configuration = configuration;
        }


        /*
         * ============================================================
         * DATABASE CONNECTION
         * ============================================================
         */
        private IDbConnection CreateConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("INVENTORY")
            );
        }


        /*
         * ============================================================
         * ITEM MASTER LIST
         * SERVER-SIDE PAGING / SEARCHING / SORTING
         * ============================================================
         */
        /*
* ============================================================
* ITEM MASTER LIST
* SERVER-SIDE PAGING / SEARCHING / SORTING
* ============================================================
*/
        public async Task<ItemMasterPagedResultDto> GetListAsync(
            string? search,
            string? groupCode,
            string? itemType,
            string active,
            int skip,
            int take,
            string sortField,
            string sortDir
        )
        {
            using var connection =
                CreateConnection();


            /*
             * ============================================================
             * PAGING VALIDATION
             * ============================================================
             */

            skip =
                Math.Max(
                    0,
                    skip
                );


            take =
                Math.Clamp(
                    take,
                    1,
                    100
                );


            /*
             * ============================================================
             * SORT FIELD VALIDATION
             * ============================================================
             *
             * Only known Item Master Grid fields are accepted.
             */

            var allowedSortFields =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                )
                {
            "itemCode",
            "itemDesc",
            "groupCode",
            "groupDesc",
            "itemType",
            "um",
            "bufferQty",
            "unitCost",
            "lastUnitCost"
                };


            if (
                string.IsNullOrWhiteSpace(
                    sortField
                )
                ||
                !allowedSortFields.Contains(
                    sortField
                )
            )
            {
                sortField =
                    "itemCode";
            }


            /*
             * Only ASC or DESC is accepted.
             */

            sortDir =
                string.Equals(
                    sortDir,
                    "desc",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? "desc"
                    : "asc";


            /*
             * ============================================================
             * ACTIVE FLAG
             * ============================================================
             */

            var normalizedActive =
                string.IsNullOrWhiteSpace(
                    active
                )
                    ? "Y"
                    : active
                        .Trim()
                        .ToUpperInvariant();


            if (
                normalizedActive != "Y"
                &&
                normalizedActive != "N"
            )
            {
                normalizedActive =
                    "Y";
            }


            /*
             * ============================================================
             * PARAMETERS
             * ============================================================
             */

            var parameters =
                new DynamicParameters();


            /*
             * Search text.
             *
             * This value is searched by SQL against the COMPLETE
             * active Item Master before SQL pagination is applied.
             */

            parameters.Add(
                "@search",
                string.IsNullOrWhiteSpace(
                    search
                )
                    ? null
                    : search.Trim(),
                DbType.String,
                size: 150
            );


            /*
             * Optional Item Group filter.
             */

            parameters.Add(
                "@group_code",
                string.IsNullOrWhiteSpace(
                    groupCode
                )
                    ? null
                    : groupCode.Trim(),
                DbType.String,
                size: 50
            );


            /*
             * Optional Item Type filter.
             */

            parameters.Add(
                "@item_type",
                string.IsNullOrWhiteSpace(
                    itemType
                )
                    ? null
                    : itemType.Trim(),
                DbType.String,
                size: 50
            );


            /*
             * Active flag.
             */

            parameters.Add(
                "@active",
                normalizedActive,
                DbType.AnsiStringFixedLength,
                size: 1
            );


            /*
             * Server-side paging.
             */

            parameters.Add(
                "@skip",
                skip,
                DbType.Int32
            );


            parameters.Add(
                "@take",
                take,
                DbType.Int32
            );


            /*
             * Server-side sorting.
             */

            parameters.Add(
                "@sort_field",
                sortField,
                DbType.AnsiString,
                size: 30
            );


            parameters.Add(
                "@sort_dir",
                sortDir,
                DbType.AnsiString,
                size: 4
            );


            /*
             * ============================================================
             * EXECUTE STORED PROCEDURE
             * ============================================================
             *
             * Result set #1:
             * Current Grid page.
             *
             * Result set #2:
             * Total matching rows across the complete search.
             */

            using var multiple =
                await connection
                    .QueryMultipleAsync(
                        "[sp].[item_master_get_list]",
                        parameters,
                        commandType:
                            CommandType.StoredProcedure,
                        commandTimeout:
                            60
                    );


            /*
             * ============================================================
             * RESULT SET #1
             * CURRENT PAGE
             * ============================================================
             */

            var items =
                (
                    await multiple
                        .ReadAsync<ItemMasterListDto>()
                )
                .AsList();


            /*
             * ============================================================
             * RESULT SET #2
             * TOTAL MATCHING RECORDS
             * ============================================================
             */


            var total =
                await multiple
                    .ReadSingleAsync<long>();


            /*
             * ============================================================
             * RESPONSE
             * ============================================================
             */

            return new ItemMasterPagedResultDto
            {
                Data =
                    items,

                Total =
                    total,

                Skip =
                    skip,

                Take =
                    take
            };
        }

        //public async Task<ItemMasterPagedResultDto> GetListAsync(
        //    string? search,
        //    string? groupCode,
        //    string? itemType,
        //    string active,
        //    int skip,
        //    int take,
        //    string sortField,
        //    string sortDir
        //)
        //{
        //    using var connection = CreateConnection();

        //    /*
        //     * Safety normalization.
        //     */
        //    skip = Math.Max(0, skip);

        //    take = Math.Clamp(
        //        take,
        //        1,
        //        100
        //    );


        //    /*
        //     * Only allow known Grid fields.
        //     *
        //     * This prevents arbitrary values from being passed
        //     * into the stored procedure.
        //     */
        //    var allowedSortFields =
        //        new HashSet<string>(
        //            StringComparer.OrdinalIgnoreCase
        //        )
        //        {
        //            "itemCode",
        //            "itemDesc",
        //            "groupCode",
        //            "groupDesc",
        //            "itemType",
        //            "um",
        //            "bufferQty",
        //            "unitCost",
        //            "lastUnitCost"
        //        };


        //    if (
        //        string.IsNullOrWhiteSpace(sortField) ||
        //        !allowedSortFields.Contains(sortField)
        //    )
        //    {
        //        sortField = "itemCode";
        //    }


        //    /*
        //     * Only ASC or DESC is allowed.
        //     */
        //    sortDir =
        //        string.Equals(
        //            sortDir,
        //            "desc",
        //            StringComparison.OrdinalIgnoreCase
        //        )
        //            ? "desc"
        //            : "asc";


        //    var parameters =
        //        new DynamicParameters();


        //    /*
        //     * Search text.
        //     */
        //    parameters.Add(
        //        "@search",
        //        string.IsNullOrWhiteSpace(search)
        //            ? null
        //            : search.Trim(),
        //        DbType.String,
        //        size: 150
        //    );


        //    /*
        //     * Optional Item Group filter.
        //     */
        //    parameters.Add(
        //        "@group_code",
        //        string.IsNullOrWhiteSpace(groupCode)
        //            ? null
        //            : groupCode.Trim(),
        //        DbType.String,
        //        size: 50
        //    );


        //    /*
        //     * Optional Item Type filter.
        //     */
        //    parameters.Add(
        //        "@item_type",
        //        string.IsNullOrWhiteSpace(itemType)
        //            ? null
        //            : itemType.Trim(),
        //        DbType.String,
        //        size: 50
        //    );


        //    /*
        //     * Active flag.
        //     */
        //    parameters.Add(
        //        "@active",
        //        string.IsNullOrWhiteSpace(active)
        //            ? "Y"
        //            : active
        //                .Trim()
        //                .ToUpperInvariant(),
        //        DbType.AnsiStringFixedLength,
        //        size: 1
        //    );


        //    /*
        //     * Paging.
        //     */
        //    parameters.Add(
        //        "@skip",
        //        skip,
        //        DbType.Int32
        //    );

        //    parameters.Add(
        //        "@take",
        //        take,
        //        DbType.Int32
        //    );


        //    /*
        //     * Sorting.
        //     */
        //    parameters.Add(
        //        "@sort_field",
        //        sortField,
        //        DbType.AnsiString,
        //        size: 30
        //    );

        //    parameters.Add(
        //        "@sort_dir",
        //        sortDir,
        //        DbType.AnsiString,
        //        size: 4
        //    );


        //    /*
        //     * Stored procedure returns TWO result sets:
        //     *
        //     * 1. Current page of Item Master records
        //     * 2. Total matching record count
        //     */
        //    using var multiple =
        //        await connection.QueryMultipleAsync(
        //            "[sp].[item_master_get_list]",
        //            parameters,
        //            commandType:
        //                CommandType.StoredProcedure,
        //            commandTimeout: 60
        //        );


        //    /*
        //     * Result set #1.
        //     */
        //    var items =
        //        (
        //            await multiple.ReadAsync<ItemMasterListDto>()
        //        )
        //        .AsList();


        //    /*
        //     * Result set #2.
        //     */
        //    var total =
        //        await multiple.ReadSingleAsync<long>();


        //    return new ItemMasterPagedResultDto
        //    {
        //        Data = items,
        //        Total = total,
        //        Skip = skip,
        //        Take = take
        //    };
        //}


        /*
         * ============================================================
         * ITEM MASTER SAVE / UPDATE
         * ============================================================
         */
        public async Task<ItemMasterUpsertResponseDto> UpsertAsync(
            ItemMasterUpsertRequestDto request
        )
        {
            using var connection = CreateConnection();

            var parameters =
                new DynamicParameters();


            parameters.Add(
                "@ITEM_CODE",
                request.ItemCode,
                DbType.String,
                size: 10
            );


            parameters.Add(
                "@ITEM_DESC",
                request.ItemDesc,
                DbType.String,
                size: 250
            );


            parameters.Add(
                "@group_code",
                request.GroupCode,
                DbType.String,
                size: 50
            );


            parameters.Add(
                "@item_type",
                request.ItemType,
                DbType.String,
                size: 50
            );


            parameters.Add(
                "@buffer_qty",
                request.BufferQty,
                DbType.Decimal
            );


            parameters.Add(
                "@unit_cost",
                request.UnitCost,
                DbType.Decimal
            );


            parameters.Add(
                "@last_unit_cost",
                request.LastUnitCost,
                DbType.Decimal
            );


            parameters.Add(
                "@um",
                request.Um,
                DbType.String,
                size: 10
            );


            parameters.Add(
                "@user_id",
                request.UserId,
                DbType.String,
                size: 10
            );


            parameters.Add(
                "@active",
                string.IsNullOrWhiteSpace(request.Active)
                    ? "Y"
                    : request.Active
                        .Trim()
                        .ToUpperInvariant(),
                DbType.AnsiStringFixedLength,
                size: 1
            );


            var row =
                await connection
                    .QuerySingleOrDefaultAsync<ItemMasterCommandRowDto>(
                        "[sp].[item_master_upsert_list]",
                        parameters,
                        commandType:
                            CommandType.StoredProcedure
                    );


            if (row is null)
            {
                return new ItemMasterUpsertResponseDto
                {
                    Success = false,

                    Message =
                        "The database did not return the saved Item Master record.",

                    Item = null
                };
            }


            if (!row.Success)
            {
                return new ItemMasterUpsertResponseDto
                {
                    Success = false,

                    Message =
                        row.Message ??
                        "The database did not confirm the Item Master save.",

                    Item = null
                };
            }


            if (
                string.IsNullOrWhiteSpace(
                    row.ItemCode
                )
            )
            {
                return new ItemMasterUpsertResponseDto
                {
                    Success = false,

                    Message =
                        "The database returned success but did not return an Item Code.",

                    Item = null
                };
            }


            return new ItemMasterUpsertResponseDto
            {
                Success = true,

                Message =
                    row.Message ??
                    "Inventory item saved successfully.",

                Item = new ItemMasterListDto
                {
                    ItemCode =
                        row.ItemCode?.Trim(),

                    ItemDesc =
                        row.ItemDesc?.Trim(),

                    GroupCode =
                        row.GroupCode?.Trim(),

                    GroupDesc =
                        row.GroupDesc?.Trim(),

                    ItemType =
                        row.ItemType?.Trim(),

                    Um =
                        row.Um?.Trim(),

                    BufferQty =
                        row.BufferQty ?? 0,

                    UnitCost =
                        row.UnitCost ?? 0,

                    LastUnitCost =
                        row.LastUnitCost ?? 0,

                    Active =
                        string.IsNullOrWhiteSpace(
                            row.Active
                        )
                            ? "Y"
                            : row.Active
                                .Trim()
                                .ToUpperInvariant()
                }
            };
        }


        /*
         * ============================================================
         * ITEM GROUP LOOKUP
         * ============================================================
         */
        public async Task<IEnumerable<ItemGroupLookupDto>>
            GetItemGroupLookupAsync(
                bool includeAll = true
            )
        {
            using var connection = CreateConnection();

            var parameters =
                new DynamicParameters();


            parameters.Add(
                "@IncludeAll",
                includeAll,
                DbType.Boolean
            );


            var result =
                await connection.QueryAsync<ItemGroupLookupDto>(
                    "dbo.sp_item_group_lookup",
                    parameters,
                    commandType:
                        CommandType.StoredProcedure
                );


            return result;
        }


        /*
         * ============================================================
         * UNIT OF MEASURE LOOKUP
         * ============================================================
         */
        public async Task<IEnumerable<ItemUmLookupDto>>
            GetItemUmLookupAsync()
        {
            using var connection = CreateConnection();


            var result =
                await connection.QueryAsync<ItemUmLookupDto>(
                    "dbo.sp_item_um_lookup",
                    commandType:
                        CommandType.StoredProcedure
                );


            return result;
        }
    }
}