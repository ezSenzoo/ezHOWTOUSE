using System;
using System.Collections.Generic;
using HOWTOUSE.DTO.Common;
using MySql.Data.MySqlClient;
using Dapper;
using System.Linq;

namespace HOWTOUSE.DAC.Common
{
    public class CommonCodeDac
    {
        /// <summary>
        /// name         : 공통코드 조회 로직
        /// desc         : 공통코드 조회 로직
        /// author       : 오승주 
        /// create date  : 2026-07-20
        /// update date  : #최종 수정 일자, 수정자, 수정개요 
        /// </summary>
        public List<CommonCodeDto> SelectCommonCodeList(string connectionString, string commonGroupCode)
        {
            const string query = @"
                                    SELECT COMN_GRP_CD,
                                           COMN_CD,
                                           COMN_CD_NM,
                                           COMN_CD_EXPL,
                                           SCRN_MRK_SEQ,
                                           USE_YN,
                                           DTRL1_NM,
                                           DTRL2_NM,
                                           DTRL3_NM,
                                           DTRL4_NM,
                                           FSR_STF_NO,
                                           FSR_DTM,
                                           LSH_STF_NO,
                                           LSH_DTM
                                      FROM CCCCCSTE
                                     WHERE COMN_GRP_CD = @ComnGrpCd
                                       AND USE_YN = 'Y'
                                     ORDER BY SCRN_MRK_SEQ";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                return connection.Query<CommonCodeDto>(
                    query,
                    new { ComnGrpCd = commonGroupCode }
                    ).ToList();
            }
        }
    }
}
