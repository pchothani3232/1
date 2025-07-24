using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace DatabaseConfiguration
{
    public partial class SalesInvoice : Form
    {
        SqlConnection con = new SqlConnection(CommClass.Connection);

        
        public SalesInvoice()
        {
            InitializeComponent();
        }

        private void SalesInvoice_Load(object sender, EventArgs e)
        {
            fillCombobox();
            AutogenerateCode();
            dataGridView2.Visible = false;
            dataGridView3.CellValidating += dataGridView3_CellValidating;
        }

        void fillCombobox()
        {
            con.Open();
            SqlCommand cmd = new SqlCommand("StateMaster_Sp", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Status", "select");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cmbState.DataSource = dt;
            cmbState.DisplayMember = "StateName";  
            //cmbState.ValueMember = "Id";
            cmbState.SelectedIndex = -1; // optional, to clear selection
            con.Close();
        }

        void AutogenerateCode()
        {
            con.Open();
            SqlCommand cmd = new SqlCommand("SalesInvoice_Sp", con);  //LoginCredential = stored procedure name
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Status", "SalesInvoiceNumber");
            SqlParameter p = new SqlParameter("@SalesInvoiceNumber", SqlDbType.Decimal);
            p.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(p);
            cmd.ExecuteNonQuery(); // Run the stored procedure

            // Show result in textbox
            txtSalesInvoiceNo.Text = p.Value.ToString();
        }


        //Search by CustomerName and MobileNumber
        private void txtCustomerName_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtCustomerName.Text.Trim();

            // Clear old selected data
            ClearCustomerData();

            if (!string.IsNullOrEmpty(searchText))
            {
                SqlCommand cmd = new SqlCommand("SalesInvoice_Sp", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Status", "SearchByNameAndMobileNo");
                cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text);
                cmd.Parameters.AddWithValue("@MobileNumber", txtCustomerName.Text);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;

                dataGridView1.Visible = true;
                con.Close();
            }
            else
            {              
                dataGridView1.Visible = false;               
            }
        }

        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
          
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                txtCustomerName.TextChanged -= txtCustomerName_TextChanged;
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtCustomerName.Text = row.Cells["Customer_Name"].Value?.ToString() ?? string.Empty;
                txtCustomerMobileNo.Text = row.Cells["Customer_PhoneNumber"].Value?.ToString() ?? string.Empty;
                txtCustomerAddress.Text = row.Cells["Customer_Address"].Value?.ToString() ?? string.Empty;
                cmbState.Text = row.Cells["Customer_State"].Value?.ToString() ?? string.Empty;
                txtCustomerName.TextChanged += txtCustomerName_TextChanged;
                dataGridView1.Visible = false;
            }
        }


        void ClearCustomerData()
        {
            txtCustomerAddress.Clear();
            txtCustomerMobileNo.Clear();
            cmbState.SelectedIndex = -1;
        }

        void ClearProductData()
        {
            txtProductCode.Clear();
            txtProductRate.Clear();
            txtProductQty.Clear();
            txtProductDiscount.Clear();
            txtProductFreeQty.Clear();                        
        }

        //RadioButton....ProductName
        private void txtProductName_TextChanged(object sender, EventArgs e)
        {

            string searchText = txtProductName.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                dataGridView2.Visible = false;
                return;
            }

            // Clear old product data
            ClearProductData();


            // Create and configure command
            SqlCommand cmd = new SqlCommand("Product_Sp", con);
            cmd.CommandType = CommandType.StoredProcedure;

            if (rbProductName.Checked)
            {
                cmd.Parameters.AddWithValue("@Status", "SearchByName");
                cmd.Parameters.AddWithValue("@Product_Name", searchText);
            }
            if (rbProductCode.Checked)// rbProductCode is selected
            {
                cmd.Parameters.AddWithValue("@Status", "SearchByCode");
                cmd.Parameters.AddWithValue("@Product_Code", searchText);
            }

            // Fetch data
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            // Display in grid
            dataGridView2.DataSource = dt;
            dataGridView2.Visible = true;

            if (con.State == ConnectionState.Open)
            con.Close();

        } 

        //RadioButton...ProductCode
        private void txtProductCode_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void rbProductCode_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void rbProductName_CheckedChanged(object sender, EventArgs e)
        {
            if (rbProductName.Checked) //Product ..Name
            {
                txtProductName_TextChanged(null, null); // Re-run name search
            }

            if (rbProductCode.Checked) //Product...Code
            {
                txtCustomerName.Clear();
                txtProductName_TextChanged(null, null); // Re-run code search
            }
        }

     
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView2.Rows.Count)
            {             
                DataGridViewRow row = dataGridView2.Rows[e.RowIndex];

                txtProductName.TextChanged -= txtProductName_TextChanged;
                txtProductCode.TextChanged -= txtProductCode_TextChanged;

                txtProductName.Text = row.Cells["Product_Name"].Value?.ToString() ?? string.Empty;
                txtProductCode.Text = row.Cells["Product_Code"].Value?.ToString() ?? string.Empty;
                txtProductRate.Text = row.Cells["Product_SalesRate"].Value?.ToString() ?? string.Empty;
              
                lblAvailableQty.Text = row.Cells["Product_AvailableQty"].Value?.ToString() ?? string.Empty;

                txtProductName.TextChanged += txtProductName_TextChanged;
                txtProductCode.TextChanged += txtProductCode_TextChanged;

                dataGridView2.Visible = false;
            }
        }

        //AddButton
        private void btnAdd_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtProductDiscount.Text))
            {
                txtProductDiscount.Text = "0";
            }
            if (string.IsNullOrWhiteSpace(txtProductQty.Text))
            {
                txtProductQty.Text = "0";
            }
            if (string.IsNullOrWhiteSpace(txtProductFreeQty.Text))
            {
                txtProductFreeQty.Text = "0";
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text) ||
                string.IsNullOrWhiteSpace(txtProductCode.Text))
            {
                MessageBox.Show("Please fill in Product Name and Code.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtProductName.Text) ||
                string.IsNullOrWhiteSpace(txtProductCode.Text) ||
                string.IsNullOrWhiteSpace(txtProductRate.Text))
            {
                MessageBox.Show("Please fill in Product Name, Code, SaleRate.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            


            if (!ValidateDiscount())
            {
                return; // Stop if any validation fails
            }


            //DataGridView3 = ExistingRow(Column)

            // Check if product already exists in the grid
            var existingRow = dataGridView3.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(row => row.Cells["ProductCode"].Value?.ToString() == txtProductCode.Text.Trim());

            if (existingRow != null)
            {
                // Step 1: Get existing values
                int oldQty = Convert.ToInt32(existingRow.Cells["ProductQty"].Value);
                int oldFreeQty = Convert.ToInt32(existingRow.Cells["ProductFreeQty"].Value);

                // Step 2: Get new values from input fields
                int newQty = Convert.ToInt32(txtProductQty.Text);
                int newFreeQty = Convert.ToInt32(txtProductFreeQty.Text);

                // Step 3: Calculate total qty after addition
                int totalQty = oldQty + newQty + oldFreeQty + newFreeQty;
                decimal availableQty = Convert.ToDecimal(existingRow.Cells["ProductAvailableQty"].Value);

                // Step 4: Validation before setting
                if (totalQty > availableQty)
                {
                    MessageBox.Show("Qty + Free Qty is greater than Available Qty. No changes applied.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    txtProductQty.Text = "";
                    txtProductFreeQty.Text = "";
                    txtProductQty.Focus();
                    return;
                }

                // Step 5: If valid, update values
                existingRow.Cells["ProductQty"].Value = oldQty + newQty;
                existingRow.Cells["ProductFreeQty"].Value = oldFreeQty + newFreeQty;


                //// Update quantity,... if already exists
                //existingRow.Cells["ProductQty"].Value = Convert.ToInt32(existingRow.Cells["ProductQty"].Value) + Convert.ToInt32(txtProductQty.Text);
                //existingRow.Cells["ProductFreeQty"].Value = Convert.ToInt32(existingRow.Cells["ProductFreeQty"].Value) + Convert.ToInt32(txtProductFreeQty.Text);

                //int TotalQty = (Convert.ToInt32(existingRow.Cells["ProductQty"].Value)) +
                //            (Convert.ToInt32(existingRow.Cells["ProductFreeQty"].Value));

                //decimal availableQty = Convert.ToDecimal(existingRow.Cells["ProductAvailableQty"].Value);
                //decimal remainingQty = availableQty - TotalQty;

                existingRow.Cells["ProductSalesRate"].Value = Convert.ToDecimal(existingRow.Cells["ProductSalesRate"].Value) + Convert.ToDecimal(txtProductRate.Text);

                // SubTotal = (ProductQty + ProductFreeQty) * ProductSaleRate                            
                Decimal ProductSubTotal = totalQty * (Convert.ToDecimal(existingRow.Cells["ProductSalesRate"].Value));
                existingRow.Cells["ProductSubTotal"].Value = ProductSubTotal;

                //Discount
                existingRow.Cells["ProductDiscount"].Value = Convert.ToDecimal(existingRow.Cells["ProductDiscount"].Value) + Convert.ToDecimal(txtProductDiscount.Text);

                //DiscountAmount = ProductSubTotal * Discount(%)
                decimal DiscountAmount = (ProductSubTotal * (Convert.ToDecimal(existingRow.Cells["ProductDiscount"].Value))) / 100;
                existingRow.Cells["DiscountAmount"].Value = DiscountAmount;

                //NetAmount = SubTotal - DiscountAmount
                decimal NetAmount = ProductSubTotal - DiscountAmount;
                existingRow.Cells["ProductNetAmount"].Value = NetAmount;

              
                    ////AvailableQty
                    //int usedQty = int.Parse(txtProductQty.Text) + int.Parse(txtProductFreeQty.Text);
                    //int availableQty = Convert.ToInt32(existingRow.Cells["ProductAvailableQty"].Value);

                    ////int updatedQty = availableQty - usedQty;

                    //// Update in Grid
                    //existingRow.Cells["ProductAvailableQty"].Value = availableQty;

                    ////// Update in Label
                    ////lblAvailableQty.Text = updatedQty.ToString();




                    clearSearchProduct();

                //2...GrossAmount
                UpdateGrossAmount();

            }
            else
            {
                // Add new row if not duplicate
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView3); // Ensure columns already created

                txtProductName.TextChanged -= txtProductName_TextChanged;
                txtProductCode.TextChanged -= txtProductCode_TextChanged;

                //2nd Gridview data showing in 3rdGridview when i click on Add..Button
                row.Cells[dataGridView3.Columns["ProductName"].Index].Value = txtProductName.Text;
                row.Cells[dataGridView3.Columns["ProductCode"].Index].Value = txtProductCode.Text;
                row.Cells[dataGridView3.Columns["ProductSalesRate"].Index].Value = txtProductRate.Text;
                row.Cells[dataGridView3.Columns["ProductQty"].Index].Value = txtProductQty.Text;
                row.Cells[dataGridView3.Columns["ProductFreeQty"].Index].Value = txtProductFreeQty.Text;
                row.Cells[dataGridView3.Columns["ProductDiscount"].Index].Value = txtProductDiscount.Text;
                row.Cells[dataGridView3.Columns["ProductAvailableQty"].Index].Value = lblAvailableQty.Text;

                int qty = Convert.ToInt32(txtProductQty.Text);
                int freeQty = Convert.ToInt32(txtProductFreeQty.Text);
                decimal rate = Convert.ToDecimal(txtProductRate.Text);
                decimal discount = Convert.ToDecimal(txtProductDiscount.Text);


                decimal ProductSubTotal = (qty + freeQty) * rate;
                row.Cells[dataGridView3.Columns["ProductSubTotal"].Index].Value = ProductSubTotal;

                decimal ProductDiscountAmount = (ProductSubTotal*discount)/ 100;
                row.Cells[dataGridView3.Columns["DiscountAmount"].Index].Value = ProductDiscountAmount;

                decimal ProductNetAmount = ProductSubTotal- ProductDiscountAmount;
                row.Cells[dataGridView3.Columns["ProductNetAmount"].Index].Value = ProductNetAmount;

                
                //// Available Qty
                //int totalUsedQty = qty + freeQty;

                //// Try to safely parse available qty from label
                //if (!decimal.TryParse(lblAvailableQty.Text, out decimal parsedQty))
                //{
                //    MessageBox.Show("Invalid available quantity. Please select a valid product first.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}

                //int originalAvailableQty = (int)parsedQty;
                ////int updatedAvailableQty = originalAvailableQty - totalUsedQty;

                //// Update grid and label with new available quantity
                //row.Cells[dataGridView3.Columns["ProductAvailableQty"].Index].Value = originalAvailableQty;
                ////lblAvailableQty.Text = updatedAvailableQty.ToString();

                txtProductName.TextChanged += txtProductName_TextChanged;
                txtProductCode.TextChanged += txtProductCode_TextChanged;

                dataGridView3.Rows.Add(row);

                clearSearchProduct();

                //2...GrossAmount
                UpdateGrossAmount();

                CalculateNetAmount();
            }
        }

        //Clear()...DataGridView3
        void clearSearchProduct()
        {
            txtProductName.Clear();
            txtProductCode.Clear();
            txtProductRate.Clear();
            txtProductQty.Clear();
            txtProductFreeQty.Clear();
            txtProductDiscount.Clear();
            lblAvailableQty.Text = "";
        }

        private void txtProductDiscount_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductDiscount.Text))
            {
                txtProductDiscount.Text = "0";
            }
        }

        //Discount
        private bool ValidateDiscount()
        {
            if (!decimal.TryParse(txtProductDiscount.Text.Trim(), out decimal discount))
            {
                MessageBox.Show("Enter a valid discount number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductDiscount.Focus();
                return false;
            }
            else if (discount < 0 || discount > 100)
            {
                MessageBox.Show("Discount must be between 0 and 100.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductDiscount.Focus();
                return false;
            }
            else if ((Convert.ToDecimal(txtProductQty.Text)+ Convert.ToDecimal(txtProductFreeQty.Text)) > Convert.ToDecimal(lblAvailableQty.Text.Trim()))
            {
                MessageBox.Show("Qty is stock of out.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtProductQty.Focus();     
                return false;
            }
          

            return true;
        }



        public void Numeric(KeyPressEventArgs e)
        {
            // Allow only digits and control characters (like backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Block the character
            }
        }

        private void txtProductQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            Numeric(e);
        }

    
        private void txtProductDiscount_KeyPress(object sender, KeyPressEventArgs e)
        {
            Numeric(e);
        }

        private void txtProductFreeQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            Numeric(e);
        }


    
        //data edit automatic update calculation
        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //3...GrossAmount
            if (e.RowIndex >= 0 && !dataGridView3.Rows[e.RowIndex].IsNewRow)
            {
                var row = dataGridView3.Rows[e.RowIndex];

                // Get values from the row
                decimal.TryParse(row.Cells["ProductSalesRate"].Value?.ToString(), out decimal rate);
                int.TryParse(row.Cells["ProductQty"].Value?.ToString(), out int qty);
                int.TryParse(row.Cells["ProductFreeQty"].Value?.ToString(), out int freeQty);
                decimal.TryParse(row.Cells["ProductDiscount"].Value?.ToString(), out decimal discount);

                int totalQty = qty + freeQty;
                decimal subtotal = totalQty * rate;

                row.Cells["ProductSubTotal"].Value = subtotal;

                decimal discountAmount = (subtotal * discount) / 100;
                row.Cells["DiscountAmount"].Value = discountAmount;

                decimal netAmount = subtotal - discountAmount;
                row.Cells["ProductNetAmount"].Value = netAmount;

                // Recalculate total gross
                UpdateGrossAmount();
                //Recalculate NetAmount
                CalculateNetAmount();
            }

        }


        //1...GrossAmount

        private void UpdateGrossAmount()
        {
            decimal grossAmount = 0;

            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                if (row.Cells["ProductSubTotal"].Value != null &&
                    decimal.TryParse(row.Cells["ProductSubTotal"].Value.ToString(), out decimal subtotal))
                {
                    grossAmount += subtotal;
                }             
            }

            txtGrossAmount.Text = grossAmount.ToString("0.00");
        }

        private void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            if (FinalDiscount()) // only if valid
            {
                CalculateNetAmount();
            }
            else
            {
                txtNetAmount.Text = "0";
                txtRoundOffDiscount.Text = "+0.00";
            }
        }

        private bool FinalDiscount()
        {
            string discountText = txtDiscount.Text.Trim();

            // Default to 0 if empty
            if (string.IsNullOrWhiteSpace(discountText))
            {
                discountText = "0";
                txtDiscount.Text = "0";  // Also update textbox for consistency
            }

            if (!decimal.TryParse(discountText, out decimal discount))
            {
                MessageBox.Show("Enter a valid discount number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiscount.Focus();
                return false;
            }
            else if (discount < 0 || discount > 100)
            {
                MessageBox.Show("Discount must be between 0 and 100.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiscount.Focus();
                return false;
            }

            return true;
        }


        private void CalculateNetAmount()
        {
            // 1. Get Gross Amount
            decimal.TryParse(txtGrossAmount.Text.Trim(), out decimal grossAmount);

            // 2. Get Discount Percentage
            decimal.TryParse(txtDiscount.Text.Trim(), out decimal discountPercent);

            // 3. Calculate discount amount
            decimal discountAmount = (grossAmount * discountPercent) / 100;

            // 4. Net amount before rounding
            decimal netBeforeRound = grossAmount - discountAmount;

            // 5. Round to nearest integer using your logic
            int netRounded;
            decimal decimalPart = netBeforeRound - Math.Truncate(netBeforeRound);
            if (decimalPart >= 0.5m)
            {
                netRounded = (int)Math.Ceiling(netBeforeRound);  // round up
            }
            else
            {
                netRounded = (int)Math.Floor(netBeforeRound);    // round down
            }

            // 6. Calculate round off difference
            decimal roundOff = netRounded - netBeforeRound;

            // 7. Show + or – in Round Off box
            string roundOffText = roundOff >= 0 ? $"+{roundOff:0.00}" : $"{roundOff:0.00}";

            // 8. Display
            txtRoundOffDiscount.Text = roundOffText;
            txtNetAmount.Text = netRounded.ToString();  // always int
        }

        private void txtRoundOffDiscount_TextChanged(object sender, EventArgs e)
        {
            if (FinalDiscount())
            {
                CalculateNetAmount();
            }
        }

        private void dataGridView3_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            UpdateGrossAmount();
            CalculateNetAmount();
        }

        private void txtDiscount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtDiscount_Validating(object sender, CancelEventArgs e)
        {
            if (!FinalDiscount())
            {
                e.Cancel = true;
            }
        }

        private void dataGridView3_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //if (e.RowIndex < 0 || dataGridView3.Rows[e.RowIndex].IsNewRow)
            //    return;

            //DataGridViewRow row = dataGridView3.Rows[e.RowIndex];
            //string columnName = dataGridView3.Columns[e.ColumnIndex].Name;
            //string value = e.FormattedValue.ToString().Trim();

            //// -------- 1. DISCOUNT VALIDATION --------
            //if (columnName == "ProductDiscount")
            //{
            //    if (string.IsNullOrWhiteSpace(value))
            //        return; // Let CellEndEdit handle default 0

            //    decimal discount;
            //    bool isValid = decimal.TryParse(value, out discount);

            //    if (!isValid || discount < 0 || discount > 100)
            //    {
            //        MessageBox.Show("Discount must be between 0 and 100.");
            //        e.Cancel = true;                    
            //    }
            //}

            //// -------- 2. QTY + FREE QTY VALIDATION --------
            //if (columnName == "ProductQty" || columnName == "ProductFreeQty")
            //{
            //    if (string.IsNullOrWhiteSpace(value))
            //        return;

            //    int qty = 0;
            //    int freeQty = 0;
            //    int availableQty = 0;

            //    if (columnName == "ProductQty")
            //    {
            //        int.TryParse(value, out qty);
            //        int.TryParse(row.Cells["ProductFreeQty"].Value?.ToString(), out freeQty);
            //    }
            //    else
            //    {
            //        int.TryParse(value, out freeQty);
            //        int.TryParse(row.Cells["ProductQty"].Value?.ToString(), out qty);
                    
            //    }

            //    int.TryParse(row.Cells["ProductAvailableQty"].Value?.ToString(), out availableQty);

            //    if ((qty + freeQty) > availableQty)
            //    {
            //        MessageBox.Show("Qty + Free Qty cannot be more than Available Qty.");
            //        e.Cancel = true;
                    
            //    }
            //}
        }
    }
}
