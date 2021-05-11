using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisionDB.Models
{
    public class NHSVoucherCode
    {
        public Guid Id { get; set; }
        public string Grade { get; set; }

        public static List<NHSVoucherCode> NHSVoucherCodes
        {
            get
            {
                return new List<NHSVoucherCode> {
                new NHSVoucherCode {
                    Id = new Guid("8D50CE47-03E7-487D-AA98-F1E38135D833"),
                    Grade = "A"
                },
                new NHSVoucherCode {
                    Id = new Guid("620A7392-8686-4E55-8EC8-FCC84955F95E"),
                    Grade = "B"
                },
                new NHSVoucherCode {
                    Id = new Guid("75AD544E-F9D7-4464-90E7-4710E9AD74F9"),
                    Grade = "C"
                },
                new NHSVoucherCode {
                    Id = new Guid("DAFB14D1-461E-4662-8F04-9F85BBC3A2D0"),
                    Grade = "D"
                },
                new NHSVoucherCode {
                    Id = new Guid("E101CD90-90A7-4D9C-A952-B401C00E175F"),
                    Grade = "E"
                },
                new NHSVoucherCode {
                    Id = new Guid("28DB1CCC-2006-4135-A578-35780EEF28C4"),
                    Grade = "F"
                },
                new NHSVoucherCode {
                    Id = new Guid("45B5205A-91C3-43D8-AD01-CAF5F0EDD506"),
                    Grade = "G"
                },
                new NHSVoucherCode {
                    Id = new Guid("C31812C0-864E-445E-806C-5406D035E7ED"),
                    Grade = "H"
                },
                new NHSVoucherCode {
                    Id = new Guid("2E1E8FF5-CD2E-4FC1-91D1-121C42751F8E"),
                    Grade = "I"
                }}.ToList();


            }
        }
    }
}