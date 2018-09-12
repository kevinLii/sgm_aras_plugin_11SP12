﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aras.IOM;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Windows.Forms; //测试弹框

namespace iku_aras_plugin
{
    public class ikuB1Utils
    {

        public List<YearLCR> yearLCR =new List<YearLCR>();
        public static List<ModelLine> getAllModelLine(Innovator inn,Item modelyear,string is_stretch)
        {
            List<ModelLine> modelline = new List<ModelLine>();
            string aml = "<Item type='ModelYear' action='get' select='year,lcr,stretch_lcr'>";
            aml +="     <id>"+modelyear.getID()+"</id>";
            aml += "	<Relationships>";
            aml += "		<Item type='Year Model' select='related_id,sort_order,id,mix,package_lcr,stretch_package_lcr,comments'>";
            aml += "			<related_id>";
            aml += "				<Item type='ModelLine' select='umd,package'/>";
            aml += "            </related_id>";
            aml += "		</Item>";
            aml += "	</Relationships>";
            aml += "</Item>";

            Item temp = inn.newItem();
            temp.loadAML(aml);
            Item results = temp.apply();
            if (results.getItemCount() > 0)
            {
                Item result = results.getItemByIndex(0);
                Item ym= result.getItemsByXPath("Relationships/Item[@type ='Year Model']");
              //  Item ml= result.getItemsByXPath("Relationships/Item[@type ='Year Model']/related_id/Item[@type='ModelLine']");

                for (int i = 0; i < ym.getItemCount(); i++)
                {
                    ModelLine m = new ModelLine();
                    m.Comments = ym.getItemByIndex(i).getProperty("comments");
                    m.PackageLCR= ym.getItemByIndex(i).getProperty("package_lcr");
                    m.Mix= ym.getItemByIndex(i).getProperty("mix");
                    m.StretchPackageLCR= ym.getItemByIndex(i).getProperty("stretch_package_lcr");
                    Item mls = ym.getItemByIndex(i).getItemsByXPath("related_id/Item[@type='ModelLine']");
                    Item ml = mls.getItemByIndex(0);
                    m.UMD = ml.getProperty("umd");
                    m.Package = ml.getProperty("package");
                    m.IS_Stretch = is_stretch;
                    modelline.Add(m);

                }
            }
              return modelline;
        }

        public static List<ModelYear> getAllModelYear(Innovator inn, Item country,string is_stretch)
        {
            List<ModelYear> modelyear = new List<ModelYear>();
            string aml = "<Item type='Location' action='get' select='id,name'>";
            aml += "    <id>" + country.getID() + "</id>";
            aml += "    <Relationships>";
            aml += "		<Item type='Location Year' select='related_id,sort_order,id'>";
            aml += "			<related_id>";
            aml += "				<Item type='ModelYear' select='year,lcr,stretch_lcr' />";
            aml += "            </related_id>";
            aml += "        </Item>";
            aml += "    </Relationships>";
            aml += "</Item>";

            Item temp = inn.newItem();
            temp.loadAML(aml);
            Item results = temp.apply();
            if (results.getItemCount() > 0)
            {
                Item result = results.getItemByIndex(0);
                Item my = result.getItemsByXPath("Relationships/Item[@type ='Location Year']/related_id/Item[@type='ModelYear']");
                for (int i=0;i<my.getItemCount();i++)
                {
                    ModelYear m = new ModelYear();
                    m.LCR = my.getItemByIndex(i).getProperty("lcr");
                    m.Year= my.getItemByIndex(i).getProperty("year");
                    m.StretchLCR = my.getItemByIndex(i).getProperty("stretch_lcr");
                    List<ModelLine> ml = getAllModelLine(inn,my.getItemByIndex(i),is_stretch);
                    m.ModelLine = ml;
                    modelyear.Add(m);
                }

            }

                return modelyear;
        }
        public static List<CountryGroup> getAllCountryGroup(Innovator inn,Item B1,string is_stretch)
        {
            List<CountryGroup> countryGroup = new List<CountryGroup>();
            //string aml = "";
            string aml = "<Item type='B1Template' action='get' select='id,program_code'>";
            aml +="     <id>" + B1.getID()+ "</id>";
            aml += "    <Relationships>";
            aml += "        <Item type='B1 Location' action='get' initial_action='GetItemConfig' select='related_id,sort_order'>";
            aml += "            <related_id>";
            aml += "  	            <Item type='Location' select='id,name'>";
            aml += "		        </Item>";
            aml += "            </related_id>";
            aml += "        </Item>";
            aml += "    </Relationships>";
            aml += "</Item>";

            Item temp = inn.newItem();
            temp.loadAML(aml);
            Item results = temp.apply();

            if (results.getItemCount() > 0)
            {
                Item result = results.getItemByIndex(0);
                Item Locations = result.getItemsByXPath("Relationships/Item[@type ='B1 Location']/related_id /Item[@type ='Location']");
                for (int i = 0; i < Locations.getItemCount(); i++)
                {
                    CountryGroup cg = new CountryGroup();
                    cg.Country = Locations.getItemByIndex(i).getProperty("name");
                    
                    List<ModelYear> my = getAllModelYear(inn, Locations.getItemByIndex(i),is_stretch);
                    cg.ModelYear = my;
                    countryGroup.Add(cg);
                }
            }
            return countryGroup;
        }
        public static List<B1Content>  getAllB1Content(Innovator inn,string cartID)
        {
            List<B1Content> b1= new List<B1Content>();

            string aml = "<Item type='B1TemplateCart' action='get' select='id,source_id,related_id'> ";
            aml += "    <id>"+ cartID + "</id>";
            aml += "    <Relationships>";
            aml += "         <Item type='B1 Template Cart' select='related_id'>";
            aml += "            <related_id>";
            aml += "                <Item type='B1Template' action='get' select='id,program_code,is_stretch '>";
            aml += "                </Item>";
            aml += "            </related_id>";            
            aml += "          </Item>";
            aml += "    </Relationships>";
            aml += "</Item>";


            Item temp = inn.newItem();
            temp.loadAML(aml);
            Item results = temp.apply();
            if (results.getItemCount() > 0)
            {
                Item result = results.getItemByIndex(0);
                Item b1Template = result.getItemsByXPath("Relationships/Item[@type ='B1 Template Cart']/related_id /Item[@type ='B1Template']");

                for (int i = 0; i< b1Template.getItemCount(); i++)
                {
                    B1Content b1Content = new B1Content();
                    b1Content.Programe = b1Template.getItemByIndex(i).getProperty("program_code");
                    b1Content.Is_Stretch= b1Template.getItemByIndex(i).getProperty("is_stretch");
                    List<CountryGroup> countrygroup = getAllCountryGroup(inn, b1Template.getItemByIndex(i), b1Template.getItemByIndex(i).getProperty("is_stretch"));
                    b1Content.CountryGroup = countrygroup;
                    b1.Add(b1Content);

                }
            
            }


            return b1;
        }

        public static List<ModelYear> 
            getAllExportModelYear(B1Content b1Content)
        {
            List<ModelYear> modelyear = new List<ModelYear>();
            List<ModelYear> tempMY = new List<ModelYear>();

            for (int i=0;i<b1Content.CountryGroup.Count;i++)
            {
                CountryGroup cg = b1Content.CountryGroup[i];
                for (int j=0;j< cg.ModelYear.Count;j++)
                {
                    ModelYear my = cg.ModelYear[j];
                    tempMY.Add(my);
                }
            }

            List<string> templist = tempMY.Select(p => p.Year).Distinct().ToList();  //year去重
            templist.Sort();

            for (int m = 0; m < templist.Count; m++)
            {
                string year = templist[m];
                List<ModelYear> thisModelYear = tempMY.FindAll(s => s.Year == year);
                List<ModelLine> tempML = new List<ModelLine>();
                ModelYear nMY = new ModelYear();
                nMY.Year = year;

                double lcr = 0;
                double stretchLCR = 0;
                for (int n = 0; n < thisModelYear.Count; n++)
                {
                    ModelYear thisMY = thisModelYear[n];
                    tempML=tempML.Concat(thisMY.ModelLine).ToList<ModelLine>();
                    lcr = lcr + double.Parse(thisMY.LCR);
                    stretchLCR= stretchLCR+ double.Parse(thisMY.StretchLCR);

                }
                nMY.LCR = lcr.ToString();
                nMY.StretchLCR = stretchLCR.ToString();

                List<string> packagelist = tempML.Select(p => p.Package).Distinct().ToList();  //package去重
                packagelist.Sort();

                List<ModelLine> tempML3 = new List<ModelLine>();

                decimal sumMix = 0;
                for (int x=0;x<packagelist.Count;x++)
                {
                    string package = packagelist[x];
                    ModelLine ml = tempML.Find(s => s.Package == package);
                    string umd = ml.UMD;
                    List<ModelLine> tempML2 = tempML.FindAll(s =>s.Package==package);
                    double pLCR = 0;
                    double spLCR = 0;
                    for (int y=0;y<tempML2.Count;y++)
                    {
                        pLCR = pLCR + double.Parse(tempML2[y].PackageLCR);
                        spLCR = spLCR + double.Parse(tempML2[y].StretchPackageLCR);
                    }
                    ModelLine nML = new ModelLine();
                    decimal mix = 0;
                    if (x== packagelist.Count-1)
                    {
                      
                        mix = 1 - sumMix;
                                          
                    }
                    else
                    {
                        mix = decimal.Parse(Math.Round(pLCR / lcr, 2).ToString());
                       sumMix = sumMix + mix;
                    }


                    nML.UMD = umd;
                    nML.Package = package;
                    mix = mix*100;
                    nML.Mix = mix.ToString();
                    nML.PackageLCR = pLCR.ToString();
                    nML.StretchPackageLCR = spLCR.ToString();
                    tempML3.Add(nML);

                }

                nMY.ModelLine = tempML3;
                modelyear.Add(nMY);
            }


            return modelyear;
        }

        public  string AppendixB1Excel(Innovator inn,string cartId,string templatefolder,string templatefile)
        {
            //创建工作簿对象
            HSSFWorkbook hssfworkbook;
            string xlsT = templatefolder + templatefile; //服务器模板本地路径

            // 打开模板文件到文件流中
            using (FileStream file = new FileStream(xlsT, FileMode.Open, FileAccess.Read))
            {
                //将文件流中模板加载到工作簿对象中
                hssfworkbook = new HSSFWorkbook(file);
            }

            OutPutXLS(hssfworkbook, inn, cartId);

            List<B1Content> b1Content = new List<B1Content>();
            b1Content = getAllB1Content(inn, cartId);
           string pgCollection = "";
            if (b1Content.Count>0)
            {
                for (int i = 0; i < b1Content.Count; i++)
                {
                    pgCollection = pgCollection + "_"+b1Content[i].Programe;
                }
            }
              string tradeTime = DateTime.Now.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);

            string fpath = templatefolder + "AppendixB1-"+ pgCollection+"-" + tradeTime + ".xls";

            FileStream outfile = new FileStream(fpath, FileMode.Create);
            hssfworkbook.Write(outfile);
            outfile.Close();  //关闭文件流  
            hssfworkbook.Close();



            return fpath;

        }

       public int ExportCount(B1Content b1content)
        {
            List<CountryGroup> cg = b1content.CountryGroup;

            List<CountryGroup> export = cg.FindAll(delegate (CountryGroup c) { return c.Country != "China"; });

            List<string> countryexport = export.Select(p => p.Country).Distinct().ToList(); //去重，暂时不用

            return export.Count;
        }

       bool is_export(List<B1Content> b1content)
        {
            bool f = false;
            for(int i = 0; i < b1content.Count; i++)
            {
                
                int exportcount = ExportCount(b1content[i]);
                if (exportcount > 1)
                {
                    f = f | true;
                }
            }


            return f;
        }
        public void OutPutXLS(HSSFWorkbook hssfworkbook,Innovator inn,string cartID)
        {
            List<B1Content> b1Content = new List<B1Content>();
            b1Content = getAllB1Content(inn, cartID);
            if (is_export(b1Content))
            {
                OutPutXLSExport(hssfworkbook, b1Content);
            }
            else
            {
                OutPutXLSLocal(hssfworkbook,b1Content);
            }
          

        }

        public void OutPutXLSLocal(HSSFWorkbook hssfworkbook, List<B1Content> b1Content)
        {
            ISheet sheet1;
            sheet1 = hssfworkbook.GetSheet("B1 Form");

            int colIndex = 4;
            int yearStart = 4;
            int countryStart = 4;
            int programeStart = 4;

            ICellStyle titleStyle = ikuNPOIUtils.currentStyle("title", hssfworkbook);
            ICellStyle style1 = ikuNPOIUtils.currentStyle("note", hssfworkbook);


            ICellStyle mixStyle = ikuNPOIUtils.currentStyle("userMix", hssfworkbook);
            ICellStyle QPUStyle = ikuNPOIUtils.currentStyle("userQPU", hssfworkbook);
            ICellStyle lcrStyle = ikuNPOIUtils.currentStyle("lcr", hssfworkbook);
            ICellStyle forStyle = ikuNPOIUtils.currentStyle("formula", hssfworkbook);


            ICell fCell = sheet1.GetRow(0).CreateCell(4);
            fCell.CellStyle = forStyle;
            string fCol = "";

            if (b1Content.Count > 0)
            {
                for (int i = 0; i < b1Content.Count; i++)
                {
                    B1Content b1 = b1Content[i];
                    List<CountryGroup> countrygroup = b1.CountryGroup;
                    for(int j = 0; j < countrygroup.Count; j++)
                    {
                        CountryGroup cg = countrygroup[j];
                        List<ModelYear> modelyear = cg.ModelYear;
                        for(int m = 0;m < modelyear.Count;m++)
                        {
                            ModelYear my = modelyear[m];
                            List<ModelLine> modelline = my.ModelLine;
                            YearLCR yl = new YearLCR(my.Year);
                            string fn = "SUM(";
                            string stretchFn = "SUM(";
                                                      
                            for (int n = 0; n < modelline.Count; n++)
                            {
                                ModelLine ml = modelline[n];

                                sheet1.AddMergedRegion(new CellRangeAddress(11, 11, colIndex, colIndex + 1));
                                sheet1.GetRow(11).CreateCell(colIndex).SetCellValue(ml.UMD);
                                sheet1.GetRow(11).GetCell(colIndex).CellStyle = titleStyle;
                                sheet1.GetRow(11).CreateCell(colIndex + 1).CellStyle = titleStyle;

                                sheet1.AddMergedRegion(new CellRangeAddress(12, 12, colIndex, colIndex + 1));
                                sheet1.GetRow(12).CreateCell(colIndex).SetCellValue(ml.Package);
                                sheet1.GetRow(12).GetCell(colIndex).CellStyle = titleStyle;
                                sheet1.GetRow(12).CreateCell(colIndex + 1).CellStyle = titleStyle;


                                sheet1.AddMergedRegion(new CellRangeAddress(13, 13, colIndex, colIndex + 1));
                                sheet1.GetRow(13).CreateCell(colIndex).SetCellValue(ml.Mix+"%");
                                sheet1.GetRow(13).GetCell(colIndex).CellStyle = titleStyle;
                                sheet1.GetRow(13).CreateCell(colIndex + 1).CellStyle = titleStyle;

                                sheet1.AddMergedRegion(new CellRangeAddress(14, 14, colIndex, colIndex + 1));
                                sheet1.GetRow(14).CreateCell(colIndex).SetCellValue("");
                                sheet1.GetRow(14).GetCell(colIndex).CellStyle = titleStyle;
                                sheet1.GetRow(14).CreateCell(colIndex + 1).CellStyle = titleStyle;


                                sheet1.GetRow(15).CreateCell(colIndex).SetCellValue("mix");
                                sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                                sheet1.GetRow(15).CreateCell(colIndex + 1).SetCellValue("QPU");
                                sheet1.GetRow(15).GetCell(colIndex + 1).CellStyle = style1;

                                string colNm1 = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex);
                                string colNm2 = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex + 1);

                                fn = fn + ml.PackageLCR + "*"+colNm1 + "17*" + colNm2 + "17+";
                                stretchFn = stretchFn+ ml.StretchPackageLCR + "*" + colNm1 + "17*" + colNm2 + "17+";

                                colIndex = colIndex + 2;

                            }
                            fn = fn.Substring(0, fn.Length - 1);
                            fn = fn + ")";
                            stretchFn = stretchFn.Substring(0, stretchFn.Length - 1);

                            stretchFn = stretchFn + ")";
                            yl.Formula = fn;
                            yl.StretchFormula = stretchFn;
                            yearLCR.Add(yl);

                            sheet1.AddMergedRegion(new CellRangeAddress(10, 10, yearStart, colIndex-1));
                            sheet1.GetRow(10).CreateCell(yearStart).SetCellValue(my.Year);
                            sheet1.GetRow(10).GetCell(yearStart).CellStyle = titleStyle;
                            for (int x=yearStart+1;x< colIndex; x++)
                            {
                                sheet1.GetRow(10).CreateCell(x);
                                sheet1.GetRow(10).GetCell(x).CellStyle = titleStyle;
                            }
                            
                            yearStart = colIndex;

                        }

                        
                        sheet1.AddMergedRegion(new CellRangeAddress(9, 9, countryStart, yearStart - 1));
                        sheet1.GetRow(9).CreateCell(countryStart).SetCellValue(cg.Country);
                        sheet1.GetRow(9).GetCell(countryStart).CellStyle = titleStyle;
                        for (int y = countryStart + 1; y < yearStart; y++)
                        {
                            sheet1.GetRow(9).CreateCell(y);
                            sheet1.GetRow(9).GetCell(y).CellStyle = titleStyle;
                        }

                        countryStart = yearStart;
                    }

                    sheet1.AddMergedRegion(new CellRangeAddress(8, 8, programeStart, countryStart - 1));
                    sheet1.GetRow(8).CreateCell(programeStart).SetCellValue(b1.Programe);
                    sheet1.GetRow(8).GetCell(programeStart).CellStyle = titleStyle;
                    for (int z = programeStart + 1; z< countryStart; z++)
                    {
                        sheet1.GetRow(8).CreateCell(z);
                        sheet1.GetRow(8).GetCell(z).CellStyle = titleStyle;
                    }

                    programeStart = countryStart;

                }

                List<YearLCR> newYearLCR = splitList(yearLCR);

                int myColIndex = colIndex;
   
                ICellStyle myStyle = ikuNPOIUtils.currentStyle("modelYear", hssfworkbook);
                sheet1.GetRow(14).CreateCell(colIndex).SetCellValue("Model Year");
                int columnWidth = sheet1.GetColumnWidth(colIndex);
                sheet1.SetColumnWidth(colIndex, columnWidth * 2);
                sheet1.GetRow(14).GetCell(colIndex).CellStyle = myStyle;
                sheet1.GetRow(15).CreateCell(colIndex);
                sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                colIndex = colIndex + 1;

                for (int a = 0; a < newYearLCR.Count; a++)
                {
                    sheet1.GetRow(14).CreateCell(colIndex).SetCellValue(newYearLCR[a].Year );
                    sheet1.SetColumnWidth(colIndex, columnWidth * 2);
                    sheet1.GetRow(14).GetCell(colIndex).CellStyle = titleStyle;
                    sheet1.GetRow(15).CreateCell(colIndex);
                    sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                    sheet1.GetRow(16).CreateCell(colIndex).SetCellFormula(newYearLCR[a].Formula);

                    string colNm= ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex);
                    fCol = fCol + colNm + ",";

                    colIndex = colIndex + 1;
                }



                if (is_stretch(b1Content))
                {
                    for (int b = 0; b < newYearLCR.Count; b++)
                    {
                        sheet1.GetRow(14).CreateCell(colIndex).SetCellValue(newYearLCR[b].Year + "\n" + "stretch");
                        sheet1.SetColumnWidth(colIndex, columnWidth * 2);
                        sheet1.GetRow(14).GetCell(colIndex).CellStyle = titleStyle;
                        sheet1.GetRow(15).CreateCell(colIndex);
                        sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                        sheet1.GetRow(16).CreateCell(colIndex).SetCellFormula(newYearLCR[b].StretchFormula);

                        string colNm = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex);
                        fCol = fCol + colNm + ",";

                        colIndex = colIndex + 1;
                    }
                }
              


                sheet1.AddMergedRegion(new CellRangeAddress(13, 13, myColIndex, colIndex - 1));
                sheet1.GetRow(13).CreateCell(myColIndex).SetCellValue("LCR");
                sheet1.GetRow(13).GetCell(myColIndex).CellStyle = titleStyle;
                for (int a = myColIndex+1; a < colIndex; a++)
                {
                    sheet1.GetRow(13).CreateCell(a);
                    sheet1.GetRow(13).GetCell(a).CellStyle = titleStyle;
                }

                for (int c = 16; c < 42; c++)
                {
                    for (int d = 4; d < colIndex; d++)
                    {
                        if (c == 16 && d > myColIndex)
                        {
                            sheet1.GetRow(c).GetCell(d).CellStyle = lcrStyle; ;
                        }
                        else if (d == myColIndex)
                        {
                            sheet1.GetRow(c).CreateCell(d).CellStyle = style1;
                        }
                        else
                        {
                            if (d % 2 == 0)
                            {
                                sheet1.GetRow(c).CreateCell(d).CellStyle = mixStyle;
                            }
                            else
                            {
                                sheet1.GetRow(c).CreateCell(d).CellStyle = QPUStyle;
                            }

                        }

                    }
                }
                fCol = fCol.Substring(0, fCol.Length - 1);
                fCell.SetCellValue(fCol);
                sheet1.CreateFreezePane(4, 0, 5, 0);
                sheet1.ProtectSheet("password");//设置密码保护
            }
        }

        public void OutPutXLSExport(HSSFWorkbook hssfworkbook, List<B1Content> b1Content)
        {
            ISheet sheet1;
            sheet1 = hssfworkbook.GetSheet("B1 Form");

            int colIndex = 4;
            int yearStart = 4;
            int countryStart = 4;
            int programeStart = 4;

            ICellStyle titleStyle = ikuNPOIUtils.currentStyle("title", hssfworkbook);
            ICellStyle mStyle = ikuNPOIUtils.currentStyle("mix", hssfworkbook);

            ICellStyle style1 = ikuNPOIUtils.currentStyle("note", hssfworkbook);


            ICellStyle mixStyle = ikuNPOIUtils.currentStyle("userMix", hssfworkbook);
            ICellStyle QPUStyle = ikuNPOIUtils.currentStyle("userQPU", hssfworkbook);
            ICellStyle lcrStyle = ikuNPOIUtils.currentStyle("lcr", hssfworkbook);
            ICellStyle forStyle = ikuNPOIUtils.currentStyle("formula", hssfworkbook);

            ICell fCell = sheet1.GetRow(0).CreateCell(4);
            fCell.CellStyle = forStyle;
            string fCol = "";


            List<B1Content> exportB1 = new List<B1Content>();
            List<B1Content> localB1 = new List<B1Content>();

            List<int> exportCols = new List<int>();
            List<int> mixCols = new List<int>();
            List<int> QPUCols = new List<int>();

            if (b1Content.Count > 0)
            {
                for (int i = 0; i < b1Content.Count; i++)
                {
                    if (ExportCount(b1Content[i]) >1)
                    {
                        exportB1.Add(b1Content[i]);
                    }
                    else
                    {
                        localB1.Add(b1Content[i]);
                    }
                }
                
                if (localB1.Count > 0)
                {
                    for (int i = 0; i < localB1.Count; i++)
                    {
                        B1Content b1 = localB1[i];
                        List<CountryGroup> countrygroup = b1.CountryGroup;
                        for (int j = 0; j < countrygroup.Count; j++)
                        {
                            CountryGroup cg = countrygroup[j];
                            List<ModelYear> modelyear = cg.ModelYear;
                            for (int m = 0; m < modelyear.Count; m++)
                            {
                                ModelYear my = modelyear[m];
                                List<ModelLine> modelline = my.ModelLine;
                                YearLCR yl = new YearLCR(my.Year);
                                string fn = "SUM(";
                                string stretchFn = "SUM(";

                                for (int n = 0; n < modelline.Count; n++)
                                {
                                    ModelLine ml = modelline[n];

                                    sheet1.AddMergedRegion(new CellRangeAddress(11, 11, colIndex, colIndex + 1));
                                    sheet1.GetRow(11).CreateCell(colIndex).SetCellValue(ml.UMD);
                                    sheet1.GetRow(11).GetCell(colIndex).CellStyle = titleStyle;
                                    sheet1.GetRow(11).CreateCell(colIndex + 1).CellStyle = titleStyle;

                                    sheet1.AddMergedRegion(new CellRangeAddress(12, 12, colIndex, colIndex + 1));
                                    sheet1.GetRow(12).CreateCell(colIndex).SetCellValue(ml.Package);
                                    sheet1.GetRow(12).GetCell(colIndex).CellStyle = titleStyle;
                                    sheet1.GetRow(12).CreateCell(colIndex + 1).CellStyle = titleStyle;


                                    sheet1.AddMergedRegion(new CellRangeAddress(13, 13, colIndex, colIndex + 1));
                                    sheet1.GetRow(13).CreateCell(colIndex).CellStyle = mStyle;
                                    sheet1.GetRow(13).CreateCell(colIndex + 1).CellStyle = mStyle;
                                    sheet1.GetRow(13).GetCell(colIndex).SetCellValue(double.Parse(ml.Mix)/100);


                                    sheet1.AddMergedRegion(new CellRangeAddress(14, 14, colIndex, colIndex + 1));
                                    sheet1.GetRow(14).CreateCell(colIndex).SetCellValue("");
                                    sheet1.GetRow(14).GetCell(colIndex).CellStyle = titleStyle;
                                    sheet1.GetRow(14).CreateCell(colIndex + 1).CellStyle = titleStyle;

                                //    sheet1.AddMergedRegion(new CellRangeAddress(15, 15, colIndex, colIndex + 1));
                                    sheet1.GetRow(15).CreateCell(colIndex).SetCellValue("mix");
                                    sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                                    mixCols.Add(colIndex);
                                    sheet1.GetRow(15).CreateCell(colIndex + 1).SetCellValue("QPU");
                                    sheet1.GetRow(15).GetCell(colIndex + 1).CellStyle = style1;
                                    QPUCols.Add(colIndex + 1);

                                    string colNm1 = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex);
                                    string colNm2 = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex + 1);

                                    fn = fn + ml.PackageLCR + "*" + colNm1 + "17*" + colNm2 + "17+";
                                    stretchFn = stretchFn + ml.StretchPackageLCR + "*" + colNm1 + "17*" + colNm2 + "17+";

                                    colIndex = colIndex + 2;

                                }
                                fn = fn.Substring(0, fn.Length - 1);
                                fn = fn + ")";
                                stretchFn = stretchFn.Substring(0, stretchFn.Length - 1);

                                stretchFn = stretchFn + ")";
                                yl.Formula = fn;
                                yl.StretchFormula = stretchFn;
                                yearLCR.Add(yl);

                                sheet1.AddMergedRegion(new CellRangeAddress(10, 10, yearStart, colIndex - 1));
                                sheet1.GetRow(10).CreateCell(yearStart).SetCellValue(my.Year);
                                sheet1.GetRow(10).GetCell(yearStart).CellStyle = titleStyle;
                                for (int x = yearStart + 1; x < colIndex; x++)
                                {
                                    sheet1.GetRow(10).CreateCell(x);
                                    sheet1.GetRow(10).GetCell(x).CellStyle = titleStyle;
                                }

                                yearStart = colIndex;

                            }


                            sheet1.AddMergedRegion(new CellRangeAddress(9, 9, countryStart, yearStart - 1));
                            sheet1.GetRow(9).CreateCell(countryStart).SetCellValue(cg.Country);
                            sheet1.GetRow(9).GetCell(countryStart).CellStyle = titleStyle;
                            for (int y = countryStart + 1; y < yearStart; y++)
                            {
                                sheet1.GetRow(9).CreateCell(y);
                                sheet1.GetRow(9).GetCell(y).CellStyle = titleStyle;
                            }

                            countryStart = yearStart;
                        }

                        sheet1.AddMergedRegion(new CellRangeAddress(8, 8, programeStart, countryStart - 1));
                        sheet1.GetRow(8).CreateCell(programeStart).SetCellValue(b1.Programe);
                        sheet1.GetRow(8).GetCell(programeStart).CellStyle = titleStyle;
                        for (int z = programeStart + 1; z < countryStart; z++)
                        {
                            sheet1.GetRow(8).CreateCell(z);
                            sheet1.GetRow(8).GetCell(z).CellStyle = titleStyle;
                        }

                        programeStart = countryStart;

                    }
                }

                if (exportB1.Count > 0)
                {
                    for (int i = 0; i < exportB1.Count; i++)
                    {
                        List<ExportModel> emList = new List<ExportModel>();
                        List<ExportModel> exportModels = new List<ExportModel>();
                        B1Content b1 = exportB1[i];

                        List<ModelYear> myList = getAllExportModelYear(b1);  //综合版部分

                        for (int m = 0; m < myList.Count; m++)
                        {
                            ModelYear my = myList[m];
                            List<ModelLine> modelline = my.ModelLine;
                          //  YearLCR yl = new YearLCR(my.Year);
                            //string fn = "SUM(";
                            //string stretchFn = "SUM(";

                            for (int n = 0; n < modelline.Count; n++)
                            {
                                ExportModel em = new ExportModel();
                                ModelLine ml = modelline[n];

                                em.UDM = ml.UMD;
                                em.Package = ml.Package;
                                em.ColIndex = colIndex;
                                em.Formula = "";
                                em.Year  = my.Year;
                                exportCols.Add(colIndex);

                                emList.Add(em);

                                int colWidth = sheet1.GetColumnWidth(colIndex);
                                sheet1.SetColumnWidth(colIndex, colWidth * 2);

                                sheet1.GetRow(11).CreateCell(colIndex).SetCellValue(ml.UMD);
                                sheet1.GetRow(11).GetCell(colIndex).CellStyle = titleStyle;
                                //sheet1.GetRow(11).CreateCell(colIndex + 1).CellStyle = titleStyle;

                                sheet1.GetRow(12).CreateCell(colIndex).SetCellValue(ml.Package);
                                sheet1.GetRow(12).GetCell(colIndex).CellStyle = titleStyle;
                                //sheet1.GetRow(12).CreateCell(colIndex + 1).CellStyle = titleStyle;

                                sheet1.GetRow(13).CreateCell(colIndex).CellStyle = mStyle;
                                //sheet1.GetRow(13).CreateCell(colIndex + 1).CellStyle = mStyle;
                                sheet1.GetRow(13).GetCell(colIndex).SetCellValue(double.Parse(ml.Mix)/100);

                                sheet1.GetRow(14).CreateCell(colIndex).SetCellValue("");
                                sheet1.GetRow(14).GetCell(colIndex).CellStyle = titleStyle;
                                //sheet1.GetRow(14).CreateCell(colIndex + 1).CellStyle = titleStyle;

                                sheet1.GetRow(15).CreateCell(colIndex).SetCellValue("QPU");
                                sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                                //sheet1.GetRow(15).CreateCell(colIndex + 1).CellStyle = style1;

                                string colNm1 = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex);
                                //string colNm2 = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex + 1);

                                //fn = fn + ml.PackageLCR + "*" + colNm1 + "17*" + colNm2 + "17+";
                                //stretchFn = stretchFn + ml.StretchPackageLCR + "*" + colNm1 + "17*" + colNm2 + "17+";

                                colIndex = colIndex + 1;

                            }
                            //fn = fn.Substring(0, fn.Length - 1);
                            //fn = fn + ")";
                            //stretchFn = stretchFn.Substring(0, stretchFn.Length - 1);

                            //stretchFn = stretchFn + ")";
                            //yl.Formula = fn;
                            //yl.StretchFormula = stretchFn;
                            //yearLCR.Add(yl);

                            sheet1.AddMergedRegion(new CellRangeAddress(10, 10, yearStart, colIndex - 1));
                            sheet1.GetRow(10).CreateCell(yearStart).SetCellValue(my.Year);
                            sheet1.GetRow(10).GetCell(yearStart).CellStyle = titleStyle;
                            for (int x = yearStart + 1; x < colIndex; x++)
                            {
                                sheet1.GetRow(10).CreateCell(x);
                                sheet1.GetRow(10).GetCell(x).CellStyle = titleStyle;
                            }

                            yearStart = colIndex;

                        }
                        sheet1.AddMergedRegion(new CellRangeAddress(9, 9, countryStart, yearStart - 1));
                        sheet1.GetRow(9).CreateCell(countryStart).SetCellValue("出口综合版");
                        sheet1.GetRow(9).GetCell(countryStart).CellStyle = titleStyle;
                        for (int y = countryStart + 1; y < yearStart; y++)
                        {
                            sheet1.GetRow(9).CreateCell(y);
                            sheet1.GetRow(9).GetCell(y).CellStyle = titleStyle;
                        }

                        countryStart = yearStart;

                        //========以下是出口正常输出的部分
                     
                        List<CountryGroup> countrygroup = b1.CountryGroup;
                        for (int j = 0; j < countrygroup.Count; j++)
                        {
                            CountryGroup cg = countrygroup[j];
                            List<ModelYear> modelyear = cg.ModelYear;
                            for (int m = 0; m < modelyear.Count; m++)
                            {
                                ModelYear my = modelyear[m];
                                List<ModelLine> modelline = my.ModelLine;
                                YearLCR yl = new YearLCR(my.Year);
                                string fn = "SUM(";
                                string stretchFn = "SUM(";

                                for (int n = 0; n < modelline.Count; n++)
                                {
                                    ModelLine ml = modelline[n];

                                    ExportModel EM = emList.Find(s => s.Year == my.Year && s.Package == ml.Package);
                   //                 MessageBox.Show("the year: "+ EM.Year+" and Package: "+ EM.Package +" ColIndex is "+EM.ColIndex );

                                    sheet1.AddMergedRegion(new CellRangeAddress(11, 11, colIndex, colIndex + 1));
                                    sheet1.GetRow(11).CreateCell(colIndex).SetCellValue(ml.UMD);
                                    sheet1.GetRow(11).GetCell(colIndex).CellStyle = titleStyle;
                                    sheet1.GetRow(11).CreateCell(colIndex + 1).CellStyle = titleStyle;

                                    sheet1.AddMergedRegion(new CellRangeAddress(12, 12, colIndex, colIndex + 1));
                                    sheet1.GetRow(12).CreateCell(colIndex).SetCellValue(ml.Package);
                                    sheet1.GetRow(12).GetCell(colIndex).CellStyle = titleStyle;
                                    sheet1.GetRow(12).CreateCell(colIndex + 1).CellStyle = titleStyle;


                                    sheet1.AddMergedRegion(new CellRangeAddress(13, 13, colIndex, colIndex + 1));
                                    sheet1.GetRow(13).CreateCell(colIndex).CellStyle = mStyle;
                                    sheet1.GetRow(13).CreateCell(colIndex + 1).CellStyle = mStyle;
                                    sheet1.GetRow(13).GetCell(colIndex).SetCellValue(double.Parse(ml.Mix) / 100);


                                    sheet1.AddMergedRegion(new CellRangeAddress(14, 14, colIndex, colIndex + 1));
                                    sheet1.GetRow(14).CreateCell(colIndex).SetCellValue("");
                                    sheet1.GetRow(14).GetCell(colIndex).CellStyle = titleStyle;
                                    sheet1.GetRow(14).CreateCell(colIndex + 1).CellStyle = titleStyle;

                         //           sheet1.AddMergedRegion(new CellRangeAddress(15, 15, colIndex, colIndex + 1));
                                    sheet1.GetRow(15).CreateCell(colIndex).SetCellValue("mix");
                                    mixCols.Add(colIndex);
                                    sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                                    sheet1.GetRow(15).CreateCell(colIndex + 1).SetCellValue("QPU");
                                    QPUCols.Add(colIndex+1);
                                    sheet1.GetRow(15).GetCell(colIndex + 1).CellStyle = style1;

                                    string colNm1 = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex);
                                    string colNm2 = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex + 1);

                                    string exportFn = EM.Formula;

                                    exportFn= exportFn+ ml.PackageLCR + "*" + colNm1 + "17*" + colNm2 + "17+";

                                    EM.Formula = exportFn;

                                    fn = fn + ml.PackageLCR + "*" + colNm1 + "17*" + colNm2 + "17+";
                                    stretchFn = stretchFn + ml.StretchPackageLCR + "*" + colNm1 + "17*" + colNm2 + "17+";

                                    colIndex = colIndex + 2;

                                }
                                fn = fn.Substring(0, fn.Length - 1);
                                fn = fn + ")";
                                stretchFn = stretchFn.Substring(0, stretchFn.Length - 1);

                                stretchFn = stretchFn + ")";
                                yl.Formula = fn;
                                yl.StretchFormula = stretchFn;
                                yearLCR.Add(yl);

                                sheet1.AddMergedRegion(new CellRangeAddress(10, 10, yearStart, colIndex - 1));
                                sheet1.GetRow(10).CreateCell(yearStart).SetCellValue(my.Year);
                                sheet1.GetRow(10).GetCell(yearStart).CellStyle = titleStyle;
                                for (int x = yearStart + 1; x < colIndex; x++)
                                {
                                    sheet1.GetRow(10).CreateCell(x);
                                    sheet1.GetRow(10).GetCell(x).CellStyle = titleStyle;
                                }

                                yearStart = colIndex;

                            }


                            sheet1.AddMergedRegion(new CellRangeAddress(9, 9, countryStart, yearStart - 1));
                            sheet1.GetRow(9).CreateCell(countryStart).SetCellValue(cg.Country);
                            sheet1.GetRow(9).GetCell(countryStart).CellStyle = titleStyle;
                            for (int y = countryStart + 1; y < yearStart; y++)
                            {
                                sheet1.GetRow(9).CreateCell(y);
                                sheet1.GetRow(9).GetCell(y).CellStyle = titleStyle;
                            }

                            countryStart = yearStart;
                        }

                        sheet1.AddMergedRegion(new CellRangeAddress(8, 8, programeStart, countryStart - 1));
                        sheet1.GetRow(8).CreateCell(programeStart).SetCellValue(b1.Programe + "-Export");
                        sheet1.GetRow(8).GetCell(programeStart).CellStyle = titleStyle;
                        for (int z = programeStart + 1; z < countryStart; z++)
                        {
                            sheet1.GetRow(8).CreateCell(z);
                            sheet1.GetRow(8).GetCell(z).CellStyle = titleStyle;
                        }

                        programeStart = countryStart;

                        for (int j=0;j<emList.Count;j++)
                        {
                            ExportModel em = emList[j];
                            string year = em.Year;
                            ModelYear my = myList.Find(s => s.Year == year);
                            List<ModelLine> mlList = my.ModelLine;
                            ModelLine ml = mlList.Find(s => s.Package == em.Package);
                            string pLCR = ml.PackageLCR;
                            string fn = em.Formula;
                            fn = fn.Substring(0, fn.Length - 1);
                            fn = "(" + fn + ")";
                            fn = fn + "/" + pLCR;
                            sheet1.GetRow(16).CreateCell(em.ColIndex).SetCellFormula(fn);
                            sheet1.GetRow(16).GetCell(em.ColIndex).CellStyle = lcrStyle;

                            string colNm = ikuNPOIUtils.ConvertColumnIndexToColumnName(em.ColIndex);
                            fCol = fCol + colNm + ",";

                        }

                    }


                }

                List<YearLCR> newYearLCR = splitList(yearLCR);

                int myColIndex = colIndex;

                ICellStyle myStyle = ikuNPOIUtils.currentStyle("modelYear", hssfworkbook);
                sheet1.GetRow(14).CreateCell(colIndex).SetCellValue("Model Year");
                int columnWidth = sheet1.GetColumnWidth(colIndex);
                sheet1.SetColumnWidth(colIndex, columnWidth * 2);
                sheet1.GetRow(14).GetCell(colIndex).CellStyle = myStyle;
                sheet1.GetRow(15).CreateCell(colIndex);
                sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                colIndex = colIndex + 1;

                for (int a = 0; a < newYearLCR.Count; a++)
                {
                    sheet1.GetRow(14).CreateCell(colIndex).SetCellValue(newYearLCR[a].Year);
                    sheet1.SetColumnWidth(colIndex, columnWidth * 2);
                    sheet1.GetRow(14).GetCell(colIndex).CellStyle = titleStyle;
                    sheet1.GetRow(15).CreateCell(colIndex);
                    sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                    sheet1.GetRow(16).CreateCell(colIndex).SetCellFormula(newYearLCR[a].Formula);

                    string colNm = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex);
                    fCol = fCol + colNm + ",";

                    colIndex = colIndex + 1;
                }

                if (is_stretch(b1Content))
                {
                    for (int b = 0; b < newYearLCR.Count; b++)
                    {
                        sheet1.GetRow(14).CreateCell(colIndex).SetCellValue(newYearLCR[b].Year + "\n" + "stretch");
                        sheet1.SetColumnWidth(colIndex, columnWidth * 2);
                        sheet1.GetRow(14).GetCell(colIndex).CellStyle = titleStyle;
                        sheet1.GetRow(15).CreateCell(colIndex);
                        sheet1.GetRow(15).GetCell(colIndex).CellStyle = style1;
                        sheet1.GetRow(16).CreateCell(colIndex).SetCellFormula(newYearLCR[b].StretchFormula);

                        string colNm = ikuNPOIUtils.ConvertColumnIndexToColumnName(colIndex);
                        fCol = fCol + colNm + ",";

                        colIndex = colIndex + 1;
                    }
                }

                sheet1.AddMergedRegion(new CellRangeAddress(13, 13, myColIndex, colIndex - 1));
                sheet1.GetRow(13).CreateCell(myColIndex).SetCellValue("LCR");
                sheet1.GetRow(13).GetCell(myColIndex).CellStyle = titleStyle;
                for (int a = myColIndex + 1; a < colIndex; a++)
                {
                    sheet1.GetRow(13).CreateCell(a);
                    sheet1.GetRow(13).GetCell(a).CellStyle = titleStyle;
                }

                //MessageBox.Show("colIndex is :" + colIndex.ToString());
                //MessageBox.Show("myColIndex is: "+myColIndex.ToString());

                for (int c = 16; c < 42; c++)
                {
                    for (int d = 4; d < colIndex; d++)
                    {

                        if (c == 16 && exportCols.Exists(s=>s==d))
                        {
                            sheet1.GetRow(c).GetCell(d).CellStyle = lcrStyle;
                            continue;
                        }
                        if (c != 16 && exportCols.Exists(s => s == d))
                        {
                            sheet1.GetRow(c).CreateCell(d).CellStyle = QPUStyle;
                            continue;
                        }
                        if (c == 16 && d > myColIndex)
                        {
                            sheet1.GetRow(c).GetCell(d).CellStyle = lcrStyle;
                            continue;
                        }
                        if (d > myColIndex)
                        {
                            sheet1.GetRow(c).CreateCell(d).CellStyle = QPUStyle ;
                            continue;
                        }
                         if (d == myColIndex)
                        {
                            sheet1.GetRow(c).CreateCell(d).CellStyle = style1;
                            continue;
                        }
                        if (mixCols.Exists(s=>s==d))
                        {
                            sheet1.GetRow(c).CreateCell(d).CellStyle = mixStyle;
                            continue;
                        }

                        if (QPUCols.Exists(s => s == d))
                        {
                            sheet1.GetRow(c).CreateCell(d).CellStyle = QPUStyle;
                            continue;
                        }


                        //}

                    }
                }
                fCol = fCol.Substring(0, fCol.Length - 1);
                fCell.SetCellValue(fCol);
                sheet1.CreateFreezePane(4, 0, 5, 0);
                sheet1.ProtectSheet("password");//设置密码保护
            }
        }

        public List<YearLCR> splitList(List<YearLCR> yearlcr)
        {
            List<YearLCR> tempList = new List<YearLCR>();
            List<YearLCR> newList = new List<YearLCR>();
            for (int i = 0; i < yearlcr.Count; i++)
            {
                YearLCR yl = yearlcr[i];
                string year = yl.Year;
                string formula = yl.Formula;
                string sFormula = yl.StretchFormula;
                year = year.Replace("，", ","); //防止中文逗号
                string[] yearArray = year.Split(',');

                for (int j = 0;j < yearArray.Length; j++)
                {
                    YearLCR newYL = new YearLCR();
                    newYL.Year= yearArray[j];
                    newYL.Formula = formula;
                    newYL.StretchFormula = sFormula;
                    tempList.Add(newYL);


                }
            }
            List<string> yearList = tempList.Select(p => p.Year).Distinct().ToList();  //year去重
            yearList.Sort();

            for (int m=0;m<yearList.Count;m++)
            {
                string year = yearList[m];
                List<YearLCR> tempYL = tempList.FindAll(s => s.Year  == year);
                YearLCR nYL = new YearLCR();
                nYL.Year = year;
                string f = " ";
                string sf=" ";
                for(int n = 0; n < tempYL.Count; n++)
                {
                    f = f + "+" + tempYL[n].Formula;
                    sf = sf + "+" + tempYL[n].StretchFormula;

                }
                f = f.Substring(2, f.Length - 2);
                sf = sf.Substring(2, sf.Length - 2);
                nYL.Formula = f;
                nYL.StretchFormula = sf;
                newList.Add(nYL);
            }


            return newList;

        }
        public List<string> Test()
        {
            List<string> templist = yearLCR.Select(p => p.Year).Distinct().ToList();  //year去重
             templist.Sort();
            return templist;

        }

       public bool is_stretch(List<B1Content> b1content)
        {
            bool f = false;
            for (int i = 0; i < b1content.Count; i++)
            {

                if (b1content[i].Is_Stretch=="1")
                {
                    f = f | true;
                }
            }


            return f;
        }

    }
}
