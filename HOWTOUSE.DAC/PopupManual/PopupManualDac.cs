using System;
using HOWTOUSE.DTO.PopupManual;
using MySql.Data.MySqlClient;
using System.Linq;
using Dapper;
using System.Collections.Generic;


namespace HOWTOUSE.DAC.PopupManual
{
    public class PopupManualDac
    {

        public List<PopupManual_INOUT> SelectPopupManualList(string connectionString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                List<PopupManual_INOUT> manuals = SelectManuals(connection);

                if (manuals.Count == 0) return manuals;

                // 메뉴얼ID를 가져옴
                List<int> manualNos = manuals.Select(manual => manual.ManuNo).ToList();


                List<PopupManualStepDto> steps = SelectSteps(connection, manualNos);
                List<PopupManualImageDto> images = SelectImages(connection, manualNos);
                List<PopupManualKeywordDto> keywords = SelectKeywords(connection, manualNos);

                foreach(PopupManual_INOUT manual in manuals)
                {
                    manual.Steps = steps
                        .Where(step => step.ManuNo == manual.ManuNo)
                        .OrderBy(step => step.StageNo)
                        .ToList();

                    manual.Images = images
                        .Where(image => image.ManuNo == manual.ManuNo)
                        .OrderBy(image => image.StageNo)
                        .ThenBy(image => image.ImageSeq)
                        .ToList();

                    manual.Keywords = keywords
                        .Where(keyword => keyword.ManuNo == manual.ManuNo)
                        .OrderBy(keyword => keyword.ScrnSortSeq)
                        .ToList();
                }

                return manuals;
            }
        }

        private static List<PopupManual_INOUT> SelectManuals(MySqlConnection connection)
        {
            const string query = @"
                                        SELECT MANU_NO,
                                               CATEGORY_CD,
                                               MANUAL_NM,
                                               MESSAGE_CNTE,
                                               PROBLEM_CNTE,
                                               ASK_STF_NM,
                                               TEL_NO,
                                               FSR_STF_NO,
                                               FSR_DTM,
                                               LSH_STF_NO,
                                               LSH_DTM
                                          FROM CRMWKLID
                                         ORDER BY LSH_DTM DESC, MANU_NO DESC";

            return connection.Query<PopupManual_INOUT>(query).ToList();
        }

        private static List<PopupManualStepDto> SelectSteps(MySqlConnection connection, List<int> manualNos)
        {
            const string query = @"
                                    SELECT MANU_NO,
                                           STAGE_NO,
                                           SOLUTION_CNTE
                                      FROM CRMWKSTD
                                     WHERE MANU_NO IN @ManualNos
                                     ORDER BY MANU_NO, STAGE_NO";

                                               
            return connection.Query<PopupManualStepDto>(query, new { ManualNos = manualNos }).ToList();
        }

        private static List<PopupManualImageDto> SelectImages(MySqlConnection connection, List<int> manualNos)
        {
            const string query = @"
                                    SELECT MANU_NO,
                                           STAGE_NO,
                                           IMAGE_SEQ,
                                           IMAGE_DATA
                                      FROM CRMWKSID
                                     WHERE MANU_NO IN @ManualNos
                                     ORDER BY MANU_NO, STAGE_NO, IMAGE_SEQ";

            return connection.Query<PopupManualImageDto>(query, new { ManualNos = manualNos }).ToList();
        }

        private static List<PopupManualKeywordDto> SelectKeywords(MySqlConnection connection, List<int> manualNos)
        {
            const string query = @"
                                    SELECT MANU_NO,
                                           KEYWORD_NM,
                                           SCRN_SORT_SEQ
                                      FROM CRMWKKED
                                     WHERE MANU_NO IN @ManualNos
                                     ORDER BY MANU_NO, SCRN_SORT_SEQ";

            return connection.Query<PopupManualKeywordDto>(query, new { ManualNos = manualNos }).ToList();
        }

        public int InsertPopupManual(string connectionString, PopupManual_INOUT manual)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int manualNo = GetNextManualNo(connection, transaction);
                        manual.ManuNo = manualNo;

                        InsertManual(connection, transaction, manual);
                        InsertSteps(connection, transaction, manual);
                        InsertImages(connection, transaction, manual);
                        InsertKeywords(connection, transaction, manual);

                        transaction.Commit();
                        return manualNo;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static int GetNextManualNo(MySqlConnection connection, MySqlTransaction transaction)
        {
            const string query = @"
SELECT IFNULL(MAX(CAST(MANU_NO AS UNSIGNED)), 0) + 1
  FROM CRMWKLID";

            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void InsertManual(MySqlConnection connection, MySqlTransaction transaction, PopupManual_INOUT manual)
        {
            const string query = @"
INSERT INTO CRMWKLID
       (MANU_NO,
        CATEGORY_CD,
        MANUAL_NM,
        MESSAGE_CNTE,
        PROBLEM_CNTE,
        ASK_STF_NM,
        TEL_NO,
        FSR_STF_NO,
        FSR_DTM,
        LSH_STF_NO,
        LSH_DTM)
VALUES (@MANU_NO,
        @CATEGORY_CD,
        @MANUAL_NM,
        @MESSAGE_CNTE,
        @PROBLEM_CNTE,
        @ASK_STF_NM,
        @TEL_NO,
        @FSR_STF_NO,
        NOW(),
        @LSH_STF_NO,
        NOW())";

            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@MANU_NO", manual.ManuNo);
                command.Parameters.AddWithValue("@CATEGORY_CD", manual.CategoryCd);
                command.Parameters.AddWithValue("@MANUAL_NM", manual.ManualNm);
                command.Parameters.AddWithValue("@MESSAGE_CNTE", manual.MessageCnte);
                command.Parameters.AddWithValue("@PROBLEM_CNTE", manual.ProblemCnte);
                command.Parameters.AddWithValue("@ASK_STF_NM", manual.AskStfNm);
                command.Parameters.AddWithValue("@TEL_NO", manual.TelNo);
                command.Parameters.AddWithValue("@FSR_STF_NO", manual.FsrStfNo);
                command.Parameters.AddWithValue("@LSH_STF_NO", manual.LshStfNo);
                command.ExecuteNonQuery();
            }
        }

        private static void InsertSteps(MySqlConnection connection, MySqlTransaction transaction, PopupManual_INOUT manual)
        {
            const string query = @"
INSERT INTO CRMWKSTD
       (MANU_NO,
        STAGE_NO,
        SOLUTION_CNTE)
VALUES (@MANU_NO,
        @STAGE_NO,
        @SOLUTION_CNTE)";

            foreach (PopupManualStepDto step in manual.Steps)
            {
                using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@MANU_NO", manual.ManuNo);
                    command.Parameters.AddWithValue("@STAGE_NO", step.StageNo);
                    command.Parameters.AddWithValue("@SOLUTION_CNTE", step.SolutionCnte);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void InsertImages(MySqlConnection connection, MySqlTransaction transaction, PopupManual_INOUT manual)
        {
            const string query = @"
INSERT INTO CRMWKSID
       (MANU_NO,
        STAGE_NO,
        IMAGE_SEQ,
        IMAGE_DATA)
VALUES (@MANU_NO,
        @STAGE_NO,
        @IMAGE_SEQ,
        @IMAGE_DATA)";

            foreach (PopupManualImageDto image in manual.Images)
            {
                using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@MANU_NO", manual.ManuNo);
                    command.Parameters.AddWithValue("@STAGE_NO", image.StageNo);
                    command.Parameters.AddWithValue("@IMAGE_SEQ", image.ImageSeq);
                    command.Parameters.Add("@IMAGE_DATA", MySqlDbType.MediumBlob).Value = image.ImageData;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void InsertKeywords(MySqlConnection connection, MySqlTransaction transaction, PopupManual_INOUT manual)
        {
            const string query = @"
INSERT INTO CRMWKKED
       (MANU_NO,
        KEYWORD_NM,
        SCRN_SORT_SEQ)
VALUES (@MANU_NO,
        @KEYWORD_NM,
        @SCRN_SORT_SEQ)";

            foreach (PopupManualKeywordDto keyword in manual.Keywords)
            {
                using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@MANU_NO", manual.ManuNo);
                    command.Parameters.AddWithValue("@KEYWORD_NM", keyword.KeywordNm);
                    command.Parameters.AddWithValue("@SCRN_SORT_SEQ", keyword.ScrnSortSeq);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
