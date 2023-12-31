using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OnlineBookstore
{
    public partial class adminbookinventory : System.Web.UI.Page
    {
        string strcon = ConfigurationManager.ConnectionStrings["con"].ConnectionString;
        static string global_filepath;
        static int global_actual_stock, global_current_stock, global_issued_books;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                fillAuthorPublisherValues();
            }
            GridView1.DataBind();

        }

        // go button click
        protected void Button4_Click1(object sender, EventArgs e)
        {
            getBookByID();
        }


        // update button click
        protected void Button3_Click(object sender, EventArgs e)
        {
            updateBookByID();
        }
        // delete button click
        protected void Button2_Click(object sender, EventArgs e)
        {
            deleteBookByID();
        }
        // add button click
        protected void Button1_Click1(object sender, EventArgs e)
        {
            if (checkIfBookExists())
            {
                Response.Write("<script>alert('Book Already Exists, try some other Book ID');</script>");
            }
            else
            {
                addNewBook();
            }
        }



        // user defined functions

        void deleteBookByID()
        {
            if (checkIfBookExists())
            {
                try
                {
                    SqlConnection con = new SqlConnection(strcon);
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    SqlCommand cmd = new SqlCommand("DELETE FROM book_master_tbl WHERE book_id=@book_id;", con);
                    cmd.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                    //SqlCommand cmd2 = new SqlCommand("DELETE from author_master_tbl WHERE book_id=@book_id;", con);
                    //cmd2.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                    SqlCommand cmd3 = new SqlCommand("DELETE FROM book_stock WHERE book_id=@book_id;", con);
                    cmd3.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                    SqlCommand cmd4 = new SqlCommand("DELETE FROM book_author WHERE book_id=@book_id;", con);
                    cmd4.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());

                    //cmd2.ExecuteNonQuery(); // author_master_tbl
                    cmd4.ExecuteNonQuery(); // book_author
                    cmd3.ExecuteNonQuery(); // book_stock
                    cmd.ExecuteNonQuery(); // book_master_tbl

                    con.Close();
                    Response.Write("<script>alert('Book Deleted Successfully');</script>");

                    GridView1.DataBind();

                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('" + ex.Message + "');</script>");
                }

            }
            else
            {
                Response.Write("<script>alert('Invalid Member ID');</script>");
            }
        }

        void updateBookByID()
        {

            if (checkIfBookExists())
            {
                try
                {

                    int actual_stock = Int32.Parse(TextBox4.Text.Trim());
                    int current_stock = Int32.Parse(TextBox5.Text.Trim());

                    if (global_actual_stock == actual_stock)
                    {

                    }
                    else
                    {
                        if (actual_stock < global_issued_books)
                        {
                            Response.Write("<script>alert('Actual Stock value cannot be less than the Issued books');</script>");
                            return;
                        }
                        else
                        {
                            current_stock = actual_stock - global_issued_books;
                            TextBox5.Text = "" + current_stock;
                        }
                    }
                    string genres = "";

                    foreach (int i in ListBox1.GetSelectedIndices())
                    {
                        genres = genres + ListBox1.Items[i] + ",";
                    }

                    genres = genres.Remove(genres.Length - 1);

                    string filepath = "~/book_inventory/books1.png";
                    filepath = "0";
                    //string filename = Path.GetFileName(FileUpload1.PostedFile.FileName);
                    //FileUpload1.SaveAs(Server.MapPath("book_inventory/" + filename));
                    //filepath = "~/book_inventory/" + filename;


                    SqlConnection con = new SqlConnection(strcon);
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }

                    SqlCommand cmd = new SqlCommand("UPDATE book_master_tbl set book_name=@book_name, genre=@genre, publisher_id=@publisher_id, publish_date=@publish_date, language=@language, edition=@edition, book_cost=@book_cost, no_of_pages=@no_of_pages, book_description=@book_description, book_img_link=@book_img_link where book_id=@book_id;", con);

                    cmd.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@book_name", TextBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@genre", genres);
                    cmd.Parameters.AddWithValue("@publisher_id", "test");
                    cmd.Parameters.AddWithValue("@publish_date", TextBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@language", DropDownList1.SelectedItem.Value);
                    cmd.Parameters.AddWithValue("@edition", TextBox9.Text.Trim());
                    cmd.Parameters.AddWithValue("@book_cost", TextBox10.Text.Trim());
                    cmd.Parameters.AddWithValue("@no_of_pages", TextBox11.Text.Trim());
                    cmd.Parameters.AddWithValue("@book_description", TextBox6.Text.Trim());
                    cmd.Parameters.AddWithValue("@book_img_link", filepath);

                    // ====================================
                    SqlCommand sqlCommand = new SqlCommand("SELECT publisher_id FROM publisher_master_tbl WHERE publisher_name=@publisher_name;", con);
                    sqlCommand.Parameters.AddWithValue("@publisher_name", DropDownList2.SelectedItem.Value);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    reader.Read();
                    String p_id = reader[0].ToString();

                    SqlCommand cmd1 = new SqlCommand("UPDATE publisher_master_tbl SET publisher_name = @publisher_name, WHERE publisher_id = @publisher_id;", con);
                    cmd1.Parameters.AddWithValue("@publisher_name", DropDownList2.SelectedItem.Value);
                    cmd1.Parameters.AddWithValue("@publisher_id", p_id);
                    reader.Close();

                    // =====================================
                    SqlCommand query = new SqlCommand("SELECT author_id FROM author_master_tbl WHERE author_name= @author_name;", con);
                    query.Parameters.AddWithValue("@author_name", DropDownList3.SelectedItem.Value);
                    reader = query.ExecuteReader();
                    reader.Read();
                    String au_id = reader[0].ToString();

                    //SqlCommand cmd2 = new SqlCommand("INSERT INTO book_author(book_id, author_id) VALUES(@book_id, @author_id);", con);
                    SqlCommand cmd2 = new SqlCommand("UPDATE book_author SET author_id = @author_id WHERE book_id=@book_id", con);
                    cmd2.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                    cmd2.Parameters.AddWithValue("@author_id", au_id);
                    reader.Close();

                    // =====================================
                    //SqlCommand cmd3 = new SqlCommand("INSERT INTO author_master_tbl(author_id, author_name) VALUES(@author_id, @author_name);", con);
                    SqlCommand cmd3 = new SqlCommand("UPDATE author_master_tbl SET author_name = @author_name, WHERE author_id = @author_id;", con);
                    cmd3.Parameters.AddWithValue("@author_name", DropDownList3.SelectedItem.Value);
                    cmd3.Parameters.AddWithValue("@author_id", au_id);

                    // =====================================

                    //SqlCommand stock_id_query = new SqlCommand("SELECT stock_id FROM book_stock WHERE book_id=@book_id;", con);
                    //stock_id_query.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                    //reader = stock_id_query.ExecuteReader();
                    //reader.Read();
                    //String s_id = reader[0].ToString();
                    //reader.Close();

                    //SqlCommand cmd4 = new SqlCommand("INSERT INTO book_stock(book_id, stock_id, actual_stock, current_stock) VALUES(@book_id, @stock_id, @actual_stock, @current_stock);", con);
                    SqlCommand cmd4 = new SqlCommand("UPDATE book_stock SET actual_stock=@actual_stock, current_stock=@current_stock WHERE book_id=@book_id;", con);
                    cmd4.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                    //cmd4.Parameters.AddWithValue("@stock_id", s_id);
                    cmd4.Parameters.AddWithValue("@actual_stock", actual_stock);
                    cmd4.Parameters.AddWithValue("@current_stock", current_stock);

                    // =====================================

                    //cmd3.ExecuteNonQuery(); //author_master_tbl
                    cmd2.ExecuteNonQuery(); //book_author
                    cmd.ExecuteNonQuery(); //book_master_tbl
                    //cmd1.ExecuteNonQuery(); //publisher_master_tbl
                    cmd4.ExecuteNonQuery(); //book_stock


                    con.Close();
                    GridView1.DataBind();
                    Response.Write("<script>alert('Book Updated Successfully');</script>");


                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('" + ex.Message + "');</script>");
                }
            }
            else
            {
                Response.Write("<script>alert('Invalid Book ID');</script>");
            }
        }


        void getBookByID() //NEED TO FIX
        {
            try
            {
                SqlConnection con = new SqlConnection(strcon);
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                SqlCommand cmd = new SqlCommand("SELECT * from book_master_tbl WHERE book_id='" + TextBox1.Text.Trim() + "';", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count >= 1)
                {
                    TextBox2.Text = dt.Rows[0]["book_name"].ToString();
                    TextBox3.Text = dt.Rows[0]["publish_date"].ToString();
                    TextBox9.Text = dt.Rows[0]["edition"].ToString();
                    TextBox10.Text = dt.Rows[0]["book_cost"].ToString().Trim();
                    TextBox11.Text = dt.Rows[0]["no_of_pages"].ToString().Trim();
                    TextBox4.Text = dt.Rows[0]["actual_stock"].ToString().Trim();
                    TextBox5.Text = dt.Rows[0]["current_stock"].ToString().Trim();
                    TextBox6.Text = dt.Rows[0]["book_description"].ToString();
                    TextBox7.Text = "" + (Int32.Parse(dt.Rows[0]["actual_stock"].ToString()) - Int32.Parse(dt.Rows[0]["current_stock"].ToString()));

                    DropDownList1.SelectedValue = dt.Rows[0]["language"].ToString().Trim();
                    DropDownList2.SelectedValue = dt.Rows[0]["publisher_name"].ToString().Trim();
                    DropDownList3.SelectedValue = dt.Rows[0]["author_name"].ToString().Trim();

                    ListBox1.ClearSelection();
                    string[] genre = dt.Rows[0]["genre"].ToString().Trim().Split(',');
                    for (int i = 0; i < genre.Length; i++)
                    {
                        for (int j = 0; j < ListBox1.Items.Count; j++)
                        {
                            if (ListBox1.Items[j].ToString() == genre[i])
                            {
                                ListBox1.Items[j].Selected = true;

                            }
                        }
                    }

                    global_actual_stock = Int32.Parse(dt.Rows[0]["actual_stock"].ToString().Trim());
                    global_current_stock = Int32.Parse(dt.Rows[0]["current_stock"].ToString().Trim());
                    global_issued_books = global_actual_stock - global_current_stock;
                    global_filepath = dt.Rows[0]["book_img_link"].ToString();

                }
                else
                {
                    Response.Write("<script>alert('Invalid Book ID');</script>");
                }

            }
            catch (Exception ex)
            {

            }
        }

        void fillAuthorPublisherValues()
        {
            try
            {
                SqlConnection con = new SqlConnection(strcon);
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                SqlCommand cmd = new SqlCommand("SELECT author_name from author_master_tbl;", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DropDownList3.DataSource = dt;
                DropDownList3.DataValueField = "author_name";
                DropDownList3.DataBind();

                cmd = new SqlCommand("SELECT publisher_name from publisher_master_table;", con);
                da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                DropDownList2.DataSource = dt;
                DropDownList2.DataValueField = "publisher_name";
                DropDownList2.DataBind();

            }
            catch (Exception ex)
            {

            }
        }

        bool checkIfBookExists()
        {
            try
            {
                SqlConnection con = new SqlConnection(strcon);
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                SqlCommand cmd = new SqlCommand("SELECT * from book_master_tbl where book_id='" + TextBox1.Text.Trim() + "' OR book_name='" + TextBox2.Text.Trim() + "';", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count >= 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
                return false;
            }
        }

        void addNewBook()
        {
            try
            {
                string genres = "";
                int actual_stock = Int32.Parse(TextBox4.Text.Trim()); 
                int current_stock = Int32.Parse(TextBox5.Text.Trim());

                foreach (int i in ListBox1.GetSelectedIndices())
                {
                    genres = genres + ListBox1.Items[i] + ",";
                }

                genres = genres.Remove(genres.Length - 1);

                string filepath = "~/book_inventory/books1.png";
                filepath = "0";
                //string filename = Path.GetFileName(FileUpload1.PostedFile.FileName);
                //FileUpload1.SaveAs(Server.MapPath("book_inventory/" + filename));
                //filepath = "~/book_inventory/" + filename;


                SqlConnection con = new SqlConnection(strcon);
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                SqlCommand book_cmd = new SqlCommand("INSERT INTO book_master_tbl(book_id) VALUES(@book_id);", con);
                book_cmd.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                book_cmd.ExecuteNonQuery();

                SqlCommand cmd = new SqlCommand("UPDATE book_master_tbl set book_name=@book_name, genre=@genre, publisher_id=@publisher_id, publish_date=@publish_date, language=@language, edition=@edition, book_cost=@book_cost, no_of_pages=@no_of_pages, book_description=@book_description, book_img_link=@book_img_link where book_id=@book_id;", con);

                cmd.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@book_name", TextBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@genre", genres);
                cmd.Parameters.AddWithValue("@publisher_id", "test");
                cmd.Parameters.AddWithValue("@publish_date", TextBox3.Text.Trim());
                cmd.Parameters.AddWithValue("@language", DropDownList1.SelectedItem.Value);
                cmd.Parameters.AddWithValue("@edition", TextBox9.Text.Trim());
                cmd.Parameters.AddWithValue("@book_cost", TextBox10.Text.Trim());
                cmd.Parameters.AddWithValue("@no_of_pages", TextBox11.Text.Trim());
                cmd.Parameters.AddWithValue("@book_description", TextBox6.Text.Trim());
                cmd.Parameters.AddWithValue("@book_img_link", filepath);

// ====================================
                SqlCommand sqlCommand = new SqlCommand("SELECT publisher_id FROM publisher_master_tbl WHERE publisher_name=@publisher_name;", con);
                sqlCommand.Parameters.AddWithValue("@publisher_name", DropDownList2.SelectedItem.Value);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                reader.Read();
                String p_id = reader[0].ToString();

                SqlCommand cmd1 = new SqlCommand("UPDATE publisher_master_tbl SET publisher_name = @publisher_name, WHERE publisher_id = @publisher_id;", con);
                cmd1.Parameters.AddWithValue("@publisher_name", DropDownList2.SelectedItem.Value);
                cmd1.Parameters.AddWithValue("@publisher_id", p_id);
                reader.Close();

                // =====================================
                SqlCommand query = new SqlCommand("SELECT author_id FROM author_master_tbl WHERE author_name= @author_name;", con);
                query.Parameters.AddWithValue("@author_name", DropDownList3.SelectedItem.Value);
                reader = query.ExecuteReader();
                reader.Read();
                String au_id = reader[0].ToString();

                SqlCommand cmd2 = new SqlCommand("INSERT INTO book_author(book_id, author_id) VALUES(@book_id, @author_id);", con);
                cmd2.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                cmd2.Parameters.AddWithValue("@author_id", au_id);
                reader.Close();

// =====================================
                SqlCommand cmd3 = new SqlCommand("INSERT INTO author_master_tbl(author_id, author_name) VALUES(@author_id, @author_name);", con);
                cmd3.Parameters.AddWithValue("@author_name", DropDownList3.SelectedItem.Value);
                cmd3.Parameters.AddWithValue("@author_id", au_id);

// =====================================

                SqlCommand query_count = new SqlCommand("SELECT COUNT(*) FROM book_stock", con);
                int count = (int)query_count.ExecuteScalar() + 1;
                String s_id = "s";
                if (count < 10)
                {
                    s_id += "00";
                    s_id += count.ToString();
                }
                else if (count < 100)
                {
                    s_id += "0";
                    s_id += count.ToString();
                }
                else
                {
                    s_id += count.ToString();
                }

                SqlCommand cmd4 = new SqlCommand("INSERT INTO book_stock(book_id, stock_id, actual_stock, current_stock) VALUES(@book_id, @stock_id, @actual_stock, @current_stock);", con);
                cmd4.Parameters.AddWithValue("@book_id", TextBox1.Text.Trim());
                cmd4.Parameters.AddWithValue("@stock_id", s_id);
                cmd4.Parameters.AddWithValue("@actual_stock", actual_stock);
                cmd4.Parameters.AddWithValue("@current_stock", current_stock);

// =====================================

                //cmd3.ExecuteNonQuery(); //author_master_tbl
                cmd2.ExecuteNonQuery(); //book_author
                cmd.ExecuteNonQuery(); //book_master_tbl
                //cmd1.ExecuteNonQuery(); //publisher_master_tbl
                cmd4.ExecuteNonQuery(); //book_stock
                con.Close();
                Response.Write("<script>alert('Book added successfully.');</script>");
                GridView1.DataBind();
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }
        }
    }
}
