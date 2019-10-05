using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace Tests
{
    public partial class Payment : Form
    {
        private SQLiteConnection DB;

        public Payment()
        {
            InitializeComponent();
        }
        private void Payment_Load(object sender, EventArgs e)
        {
            DB = new SQLiteConnection("Data Source = TestDB.db; Version=3;");//Подключение к БД
            DB.Open();//Открытие коннекта

            int m = 1;
            for (int i = 0; i < m; i++)
            {
                cmbbxCategories.Items.Clear();
            }

            /// <summary>
            ///  Вывод списка
            ///  категорий в cmbbxCategories
            /// </summary>
            for (int i = 0; i < m; i++)
            {
                SQLiteCommand command6 = new SQLiteCommand("Select name_category from category", DB);
                SQLiteDataReader sqlReader6 = null;
                sqlReader6 = command6.ExecuteReader();
                while (sqlReader6.Read())
                {
                    cmbbxCategories.Items.Add(Convert.ToString(sqlReader6["name_category"]));
                }
                if (sqlReader6 != null)
                    sqlReader6.Close();
                command6.ExecuteNonQuery();
            }

            SQLiteCommand command5 = new SQLiteCommand("Select * from employees", DB);//Запрос на вывод всей таблицы employees
            SQLiteDataReader sqlReader5 = null;
            sqlReader5 = command5.ExecuteReader();


            /// <summary>
            ///  Вывод таблицы
            ///  employees в dgvEmployees
            /// </summary>
            while (sqlReader5.Read())
            {
                SQLiteCommand cmd1 = DB.CreateCommand();
                cmd1.CommandText = "select name_category from category where num_category = '" + sqlReader5["category"] + "'";
                string cat = cmd1.ExecuteScalar().ToString();
                cmd1.ExecuteNonQuery();

                dgvEmployees.Rows.Add(Convert.ToInt32(sqlReader5["num_employee"]),sqlReader5["surname"], sqlReader5["firstname"],sqlReader5["middlename"],
                cat, sqlReader5["start_date"]);
            }    
            if (sqlReader5 != null)
                sqlReader5.Close();

            command5.ExecuteNonQuery();


            /// <summary>
            ///  Вывод списка руководителей и
            ///  их категорий, суммы з/п подчинённых за 3 месяца,
            /// макс., мин. и ср. з/п подчинённых за полгода
            /// </summary>
            SQLiteCommand command = new SQLiteCommand("SELECT employees.surname || ' '|| employees.firstname || ' ' || employees.middlename AS [SFM], "+
                                                   "category.name_category as [categorys], sum(salary.sal) AS[Sum],"+
                                                   "(SELECT max(salary.sal) || ' р.- max, ' || min(salary.sal) || ' р.- min, ' || round(avg(salary.sal), 2)||' р.- avg'" +
                                                        "WHERE salary.payday BETWEEN @date1 AND @date2) AS [MMF]"+
                                                    " FROM category INNER JOIN employees ON category.num_category = employees.category INNER JOIN "+
                                                   "leaders ON employees.num_employee = leaders.id_leader INNER JOIN salary ON leaders.id_subordinate = " +
                                                   "salary.num_employee WHERE salary.payday BETWEEN @date3 AND @date4 GROUP BY id_leader ORDER BY category.name_category DESC ", DB);

            command.Parameters.Add("@date1", DbType.String).Value = DateTime.Today.AddMonths(-6).ToString("yyyy-MM-dd");//вычетание 6 месяцев из текущей даты
            command.Parameters.Add("@date2", DbType.String).Value = DateTime.Now.ToString("yyyy-MM-dd");
            command.Parameters.Add("@date3", DbType.String).Value = DateTime.Today.AddMonths(-3).ToString("yyyy-MM-dd");//вычетание 3 месяцев из текущей даты
            command.Parameters.Add("@date4", DbType.String).Value = DateTime.Now.ToString("yyyy-MM-dd");
            SQLiteDataReader sqlReader = null;
            
            sqlReader = command.ExecuteReader();
            while (sqlReader.Read())
            {
                dtgvPaySub.Rows.Add(sqlReader["SFM"], sqlReader["categorys"], sqlReader["Sum"], sqlReader["MMF"]);
            }
            if (sqlReader != null)
                sqlReader.Close();

            command.ExecuteNonQuery();


            /// <summary>
            ///  Отображение всех сотрудников компании и 
            ///  их зарплаты за полгода(месяца горизонтально)
            /// </summary>
            SQLiteCommand command2 = new SQLiteCommand(" select employees.surname||' '||employees.firstname||' '||employees.middlename as 'SFM2'," +
" group_concat((select salary.sal where payday like '%'||strftime('%Y-%m', date('now','start of month','-5 month'))||'%')) as 'tecdate_6'," +
" group_concat((select salary.sal  where payday like '%'||strftime('%Y-%m', date('now','start of month','-4 month'))||'%')) as 'tecdate_5'," +
" group_concat((select salary.sal where payday like '%'||strftime('%Y-%m', date('now','start of month','-3 month'))||'%')) as 'tecdate_4'," +
" group_concat((select salary.sal  where payday like '%'||strftime('%Y-%m', date('now','start of month','-2 month'))||'%')) as 'tecdate_3'," +
" group_concat((select salary.sal where payday like '%'||strftime('%Y-%m', date('now','start of month','-1 month'))||'%')) as 'tecdate_2'," +
" group_concat((select salary.sal  where payday like '%'||strftime('%Y-%m', date('now'))||'%')) as 'tecdate'" +
" from salary inner join employees on salary.num_employee=employees.num_employee group by employees.num_employee", DB);

            dgvZpPolGod.Columns[1].HeaderText = DateTime.Today.AddMonths(-5).ToString("yyyy-MM");
            dgvZpPolGod.Columns[2].HeaderText = DateTime.Today.AddMonths(-4).ToString("yyyy-MM");
            dgvZpPolGod.Columns[3].HeaderText = DateTime.Today.AddMonths(-3).ToString("yyyy-MM");
            dgvZpPolGod.Columns[4].HeaderText = DateTime.Today.AddMonths(-2).ToString("yyyy-MM");
            dgvZpPolGod.Columns[5].HeaderText = DateTime.Today.AddMonths(-1).ToString("yyyy-MM");
            dgvZpPolGod.Columns[6].HeaderText = DateTime.Today.ToString("yyyy-MM");
            SQLiteDataReader sqlReader2 = null;
            sqlReader2 = command2.ExecuteReader();
            while (sqlReader2.Read())
            {
                dgvZpPolGod.Rows.Add(sqlReader2["SFM2"], sqlReader2["tecdate_6"], sqlReader2["tecdate_5"], sqlReader2["tecdate_4"], sqlReader2["tecdate_3"], sqlReader2["tecdate_2"], sqlReader2["tecdate"]);
            }
            if (sqlReader2 != null)
                sqlReader2.Close();

            command2.ExecuteNonQuery();

            ///<summary>
            ///Вывод среднего размера зарплат 
            ///в рамках должности (группы сотрудников)
            ///</summary>
            SQLiteCommand command3 = new SQLiteCommand(" select category.name_category as 'nameCategory',round(avg(salary.sal),2) "+
                "as 'avgPay' from salary inner join employees on salary.num_employee=employees.num_employee "+
                "inner join category on employees.category = category.num_category group by category.name_category "+
                "order by category.name_category desc", DB);

            SQLiteDataReader sqlReader3 = null;

            sqlReader3 = command3.ExecuteReader();
            while (sqlReader3.Read())
            {
                dtgAvg.Rows.Add(sqlReader3["nameCategory"], sqlReader3["avgPay"]);
            }
            if (sqlReader3 != null)
                sqlReader3.Close();

            command3.ExecuteNonQuery();
        }

        /// <summary>
        /// добавление сотрудников в таблицу 'Employees' 
        /// </summary>
        private void addEployee_Click_1(object sender, EventArgs e)
        {
            int cod;
            if (txtFirstName.Text != "" && txtSurName.Text != "" && txtMiddleName.Text != "" && cmbbxCategories.Text != "")
            {
                SQLiteCommand cmd1 = DB.CreateCommand();
                cmd1.CommandText = "select num_category from category where name_category = '" + cmbbxCategories.Text + "'";
                cod = Convert.ToInt32(cmd1.ExecuteScalar().ToString());
                SQLiteCommand cmd = DB.CreateCommand();
                cmd.CommandText = "INSERT INTO employees (surname,firstname,middlename,category,start_date) VALUES (@surname,@firstname,@middlename,@category,@start_day);";
                cmd.Parameters.Add("@surname", System.Data.DbType.String).Value = txtSurName.Text;
                cmd.Parameters.Add("@firstname", System.Data.DbType.String).Value = txtFirstName.Text;
                cmd.Parameters.Add("@middlename", System.Data.DbType.String).Value = txtMiddleName.Text;
                cmd.Parameters.Add("@category", System.Data.DbType.String).Value = cod;
                cmd.Parameters.Add("@start_day", System.Data.DbType.String).Value = dateStart.Text;
                cmd.ExecuteNonQuery();
                cmd1.ExecuteNonQuery();
                MessageBox.Show("Выполнено добавление", "Добавление");
            }
            else
            {
                MessageBox.Show("Введите все параметры", "Ошибка");
            }
        }

        /// <summary>
        /// для поисковика по сотрудникам на вкладке 'Расчёт зарплаты'
        /// </summary>
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            for (int i = 0; i <= dgvEmployees.Rows.Count - 1; i++)
            {
                for (int j = 0; j <= dgvEmployees.ColumnCount - 1; j++)
                {
                    if (dgvEmployees.Rows[i].Cells[0].Value != null || dgvEmployees.Rows[i].Cells[1].Value != null ||
                        dgvEmployees.Rows[i].Cells[2].Value != null || dgvEmployees.Rows[i].Cells[3].Value != null || 
                        dgvEmployees.Rows[i].Cells[4].Value != null || dgvEmployees.Rows[i].Cells[5].Value != null)
                    {
                        if (dgvEmployees.Rows[i].Cells[0].Value.ToString().Contains(txtSearch.Text.ToUpper()) || 
                            (dgvEmployees.Rows[i].Cells[1].Value.ToString().Contains(txtSearch.Text.ToUpper())) || 
                            (dgvEmployees.Rows[i].Cells[2].Value.ToString().Contains(txtSearch.Text.ToUpper())) || 
                            (dgvEmployees.Rows[i].Cells[3].Value.ToString().Contains(txtSearch.Text.ToUpper())) || 
                            (dgvEmployees.Rows[i].Cells[4].Value.ToString().Contains(txtSearch.Text.ToUpper())) || 
                            (dgvEmployees.Rows[i].Cells[5].Value.ToString().Contains(txtSearch.Text.ToUpper())))
                        {
                            dgvEmployees.Rows[i].Visible = true;
                            break;
                        }
                        else { dgvEmployees.Rows[i].Visible = false; }
                    }
                }
            }
        }

        /// <summary>
        /// для поисковика по сотрудникам на вкладке 'Расчёт зарплаты'
        /// </summary>
        private void txtSearch_Leave(object sender, EventArgs e)
        {
            txtSearch.Text = "Поиск...";
            txtSearch.ForeColor = Color.Gray;
        }
        /// <summary>
        /// для поисковика по сотрудникам на вкладке 'Расчёт зарплаты'
        /// </summary>
        private void txtSearch_Enter(object sender, EventArgs e)
        {
            txtSearch.Text = null;
            txtSearch.ForeColor = Color.Black;
        }

        int iD;
        string surname, firstname, middlename, categ, date;


        /// <summary>
        /// вывод суммы зарплаты за сотрудника и всех сотрудников за выбранный день
        /// </summary>
        private void bttnPayment_Click_1(object sender, EventArgs e)
        {
            double payment, premium, sum_sub;
            int z = 0;

            lblPayment.Text = "Сумма зарплаты сотрудника равна: ";
            lblPaymentSum.Text = "Сумма зарплаты всех сотрудников фирмы: ";
            if (dgvEmployees.SelectedRows.Count > 0)
            {

                ///<sammary>
                ///принятие данных из dgvEmployees
                ///</sammary>
                DataGridViewRow selectedRow = dgvEmployees.Rows[dgvEmployees.SelectedCells[0].RowIndex];//id
                DataGridViewRow selectedRow0 = dgvEmployees.Rows[dgvEmployees.SelectedCells[1].RowIndex];//фамилия
                DataGridViewRow selectedRow1 = dgvEmployees.Rows[dgvEmployees.SelectedCells[2].RowIndex];//имя
                DataGridViewRow selectedRow2 = dgvEmployees.Rows[dgvEmployees.SelectedCells[3].RowIndex];//отчество
                DataGridViewRow selectedRow3 = dgvEmployees.Rows[dgvEmployees.SelectedCells[4].RowIndex];//категория
                DataGridViewRow selectedRow4 = dgvEmployees.Rows[dgvEmployees.SelectedCells[5].RowIndex];//дата начала работы

                ///<summary>
                ///присваивание переменными значений из dgvEmployees
                ///</summary>
                iD = Convert.ToInt32(selectedRow.Cells["num"].Value);
                surname = Convert.ToString(selectedRow0.Cells["Surn"].Value);
                firstname = Convert.ToString(selectedRow1.Cells["Firstn"].Value);
                middlename = Convert.ToString(selectedRow2.Cells["Middlen"].Value);
                categ = Convert.ToString(selectedRow3.Cells["category"].Value);
                date = Convert.ToDateTime(selectedRow4.Cells["start_date"].Value).Year.ToString();

                SQLiteCommand cmd1 = DB.CreateCommand();
                cmd1.CommandText = "select salary from category where name_category = '" + categ + "'";//вывод стандартной суммы в категории
                payment = Convert.ToDouble(cmd1.ExecuteScalar());
                cmd1.ExecuteNonQuery();
                int year = Convert.ToInt32(DateTime.Now.Year.ToString());
                int dateEmpl = Convert.ToInt32(date);

                ///<summary>
                ///расчёт з/п в зависимости от категории
                ///</summary>
                try
                {
                    if (categ == "Employee")//расчёт для категории 'Employee'
                    {
                        if (year != dateEmpl)
                        {
                            while (z != year - dateEmpl)
                            {
                                if (z <= 10)
                                {
                                    premium = payment * 0.03;
                                    payment += premium;
                                    z++;
                                }
                                else
                                    break;
                            }
                        }
                    }
                    else if (categ == "Manager")//расчёт для категории 'Manager'
                    {
                        if (year != dateEmpl)
                        {
                            while (z != year - dateEmpl)
                            {
                                if (z <= 8)
                                {
                                    premium = payment * 0.05;
                                    payment += premium;
                                    z++;
                                }
                                else
                                    break;
                            }
                            SQLiteCommand cmd4 = DB.CreateCommand();
                            cmd4.CommandText = "SELECT sum(salary.sal)*0.005 as sal_sub FROM salary inner join employees "+
                                                "on salary.num_employee = employees.num_employee inner join "+
                                                "leaders on employees.num_employee = leaders.id_leader where "+
                                                "leaders.id_leader = @id and salary.payday = @date";//запрос на расчёт надбавки зарплаты за подчинённых
                            cmd4.Parameters.Add("@id", DbType.String).Value = iD;
                            cmd4.Parameters.Add("@date", DbType.String).Value = dtpSearch.Text;
                            sum_sub = Convert.ToDouble(cmd4.ExecuteScalar());
                            cmd4.ExecuteNonQuery();
                            payment += sum_sub;
                        }
                    }
                    else if (categ == "Salesman")//расчёт для категории 'Salesman'
                    {

                        if (year != dateEmpl)
                        {
                            while (z != year - dateEmpl)
                            {
                                if (z <= 35)
                                {
                                    premium = payment * 0.01;
                                    payment += premium;
                                    z++;
                                }
                                else
                                    break;
                            }
                            SQLiteCommand cmd4 = DB.CreateCommand();
                            cmd4.CommandText = "SELECT sum(salary.sal)*0.005 as sal_sub FROM salary inner join employees " +
                                                "on salary.num_employee = employees.num_employee inner join " +
                                                "leaders on employees.num_employee = leaders.id_leader where " +
                                                "leaders.id_leader = @id and salary.payday = @date";//запрос на расчёт надбавки зарплаты за подчинённых
                            cmd4.Parameters.Add("@id", DbType.String).Value = iD;
                            cmd4.Parameters.Add("@date", DbType.String).Value = dtpSearch.Text;
                            sum_sub = Convert.ToDouble(cmd4.ExecuteScalar());
                            cmd4.ExecuteNonQuery();
                            payment += sum_sub;
                        }
                    }

                    double obchSum;
                    lblPayment.Text += Math.Round(payment, 2) + " рублей";//вывод заработной платы выбранного сотрудника за выбранную дату

                    SQLiteCommand cmd5 = DB.CreateCommand();
                    cmd5.CommandText = "SELECT sum(salary.sal)as sal_sub FROM salary where payday=@date";
                    cmd5.Parameters.Add("@date", DbType.String).Value = dtpSearch.Text;
                    obchSum = Convert.ToDouble(cmd5.ExecuteScalar());
                    cmd5.ExecuteNonQuery();
                    lblPaymentSum.Text += Math.Round(obchSum, 2) + " рублей";//вывод заработной платы всех сотрудников за выбранную дату
                }
                catch { MessageBox.Show("За данное время нет зачисления з/п", "Исключение"); }
            }
        }

        /// <summary>
        /// вывод суммы зарплаты за определённый период
        /// </summary>
        private void bttnPaymentSum_Click(object sender, EventArgs e)
        {
            try
            {
                sumSotr.Text = string.Empty;
                DataGridViewRow selectedRow = dgvEmployees.Rows[dgvEmployees.SelectedCells[0].RowIndex];//id
                iD = Convert.ToInt32(selectedRow.Cells["num"].Value);
                SQLiteCommand cmd6 = DB.CreateCommand();
                cmd6.CommandText = "SELECT sum(salary.sal)as sal_sub FROM salary where salary.num_employee=@id and payday>=@date1 AND payday<=@date2";
                cmd6.Parameters.Add("@id", DbType.String).Value = iD;
                cmd6.Parameters.Add("@date1", DbType.String).Value = dtTpOt.Text;
                cmd6.Parameters.Add("@date2", DbType.String).Value = dtTpDo.Text;
                string sum = cmd6.ExecuteScalar().ToString();
                cmd6.ExecuteNonQuery();
                sumSotr.Text += sum + " рублей";
            }
            catch { MessageBox.Show("За данный период времени нет данных","Сообщение"); }
        }
    }
}
