using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers {
    [ApiController]
    [Route ("[controller]")]
    public class UploadController : ControllerBase {
        SqlConnection con;

        [HttpPost]
        public IActionResult Post () {
            try {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine ("Resources", "Files");
                var pathToSave = Path.Combine (Directory.GetCurrentDirectory (), folderName);
                if (file.Length > 0) {
                    var fileName = ContentDispositionHeaderValue.Parse (file.ContentDisposition).FileName.Trim ('"');
                    var fullPath = Path.Combine (pathToSave, fileName);
                    var dbPath = Path.Combine (folderName, fileName);
                    using (var stream = new FileStream (fullPath, FileMode.Create)) {
                        file.CopyTo (stream);
                    }

                    //Creating object of datatable  
                    DataTable tblcsv = new DataTable ();
                    //creating columns  
                    tblcsv.Columns.Add ("Name");
                    tblcsv.Columns.Add ("City");
                    tblcsv.Columns.Add ("Address");
                    tblcsv.Columns.Add ("Designation");
                    //getting full file path of Uploaded file  
                    string CSVFilePath = Path.GetFullPath (fileName);
                    //Reading All text  
                    string ReadCSV = System.IO.File.ReadAllText (CSVFilePath);
                    //spliting row after new line  
                    foreach (string csvRow in ReadCSV.Split ('\n')) {
                        if (!string.IsNullOrEmpty (csvRow)) {
                            //Adding each row into datatable  
                            tblcsv.Rows.Add ();
                            int count = 0;
                            foreach (string FileRec in csvRow.Split (',')) {
                                tblcsv.Rows[tblcsv.Rows.Count - 1][count] = FileRec;
                                count++;
                            }
                        }

                    }
                    //Calling insert Functions  
                    InsertCSVRecords (tblcsv);
                    return Ok ("File uploaded Successfully");
                } else {
                    return BadRequest ();
                }
            } catch (Exception ex) {
                return StatusCode (500, $"Internal server error: {ex}");
            }
        }

    private void connection () {
        sqlconn = ConfigurationManager.ConnectionStrings["SqlCom"].ConnectionString;
        con = new SqlConnection (sqlconn);

    }

    private void InsertCSVRecords(DataTable csvdt)  
    {  
  
        connection();  
        //creating object of SqlBulkCopy    
        SqlBulkCopy objbulk = new SqlBulkCopy(con);  
        //assigning Destination table name    
        objbulk.DestinationTableName = "Employee";  
        //Mapping Table column    
        objbulk.ColumnMappings.Add("Name", "Name");  
        objbulk.ColumnMappings.Add("City", "City");  
        objbulk.ColumnMappings.Add("Address", "Address");  
        objbulk.ColumnMappings.Add("Designation", "Designation");  
        //inserting Datatable Records to DataBase    
        con.Open();  
        objbulk.WriteToServer(csvdt);  
        con.Close();  
  
  
    }  
    }
}