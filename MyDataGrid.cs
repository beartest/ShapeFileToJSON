using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace geometry_json_translate
{
    public class DataColumn    
    {
        #region "Properties"     
        /// <summary>        
        /// 列名        
        /// </summary>        
        public string ColumnName { get; set; }     
        /// <summary>      
        /// 类型        
        /// </summary>       
        public Type DataType { get; set; }        
        /// <summary>       
        /// 列标题       
        /// </summary>        
        public string Caption { get; set; }      
        /// <summary>        
        /// 是否允许用户改变列的大小       
        /// </summary>      
        public bool AllowResize { get; set; }        
        /// <summary>        
        /// 是否允许用户进行排序       
        /// </summary>       
        public bool AllowSort { get; set; }       
        /// <summary>        
        /// 是否允许用户进行重新排序       
        /// </summary>        
        public bool AllowReorder { get; set; }      
        /// <summary>      
        /// 是否只读       
        /// </summary>      
        public bool ReadOnly { get; set; }        
        #endregion     
  
        /// <summary>       
        /// 构造并且赋初始值      
        /// </summary>       
        /// <param name="columnName">列名</param>     
         public DataColumn(string columnName)      
         {          
             this.ColumnName = columnName;      
             this.Caption = columnName;          
             this.AllowResize = true;        
             this.AllowSort = true;          
             this.AllowReorder = true;       
             this.ReadOnly = false;      
         }      
        ///<summary>        
        ///重载构造     
        ///</summary>       
        ///<param name="columnName">列名</param>       
        ///<param name="caption">列标题</param>       
        ///<param name="allowResize">是否允许改变列大小</param>    
        ///<param name="allowSort">是否允许排序</param>      
        ///<param name="allowReorder">是否允许重新排序</param>       
        ///<param name="readOnly">列只读</param>        
        public DataColumn(string columnName, string caption, bool allowResize, bool allowSort, bool allowReorder, bool readOnly)      
        {            
            this.ColumnName = columnName;        
            this.Caption = caption;           
            this.AllowResize = allowResize;     
            this.AllowSort = allowSort;        
            this.AllowReorder = allowReorder;   
            this.ReadOnly = readOnly;       
        }   
    }

    /// <summary>  
    /// DataColumn集合，继承与list  
    /// </summary>  
    public class DataColumnCollection : List<DataColumn>   
    {       
        /// <summary>      
        /// 隐藏List类中add方法，重新定义Add方法,判断有重复列的时候报出异常       
        /// </summary>       
        /// <param name="dc"></param>        
        public new void Add(DataColumn dc)   
        {          
            foreach (DataColumn curColumn in this)      
            {            
                if (dc.ColumnName == curColumn.ColumnName)     
                {                  
                    throw new Exception(String.Format("该列已经存在", dc.ColumnName));               
                }          
            }          
            base.Add(dc);        
        }    
    }

    public class DataRow  
    {      
        public Dictionary<string, object> items { set; get; }      
        public DataRow()       
        {         
            this.items = new Dictionary<string, object>();     
        }      
        /// <summary>       
        /// DataRow类索引器 （DataRow[.....]）     
        /// </summary>      
        /// <param name="key"></param>      
        /// <returns></returns>       
        public object this[string key]   
        {          
            set { items[key] = value; }    
            get { return items[key]; }     
        }        
        /// <summary>      
        /// 通过emit反射在内存中创建出一个包含属性的类      
        /// </summary>       
        /// <returns></returns>    
        public Assembly EmitAssembly()       
        {           
            AssemblyName assemblyName = new AssemblyName("DataRowAssembly");   
            AssemblyBuilder assemblyBuilder=Thread.GetDomain().DefineDynamicAssembly(assemblyName,AssemblyBuilderAccess.Run);            
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DataRowModel",true);            TypeBuilder typeBuilder = moduleBuilder.DefineType("DataRowObject",TypeAttributes.Public|TypeAttributes.Class);        
            foreach(KeyValuePair<string, object>  pair in items)             
            {           
                BuilderFieldsAndProperty(typeBuilder, pair.Key,pair.Value.GetType());       
            }            
            typeBuilder.CreateType();     
            return assemblyBuilder;       
        }        
        /// <summary>        
        /// 通过emit反射创建字段和属性      
        /// </summary>       
        /// <param name="myTypeBuilder">TypeBuilder</param>       
        /// <param name="name">需要创建的属性名</param>      
        /// <param name="type">包含该属性的类的类型</param>      
        public void BuilderFieldsAndProperty(TypeBuilder myTypeBuilder, string name, Type type)         
        {         
            FieldBuilder myFieldBuilder = myTypeBuilder.DefineField(name, type, FieldAttributes.Private); 
            PropertyBuilder myPropertyBuilder = myTypeBuilder.DefineProperty(name.ToUpper(), PropertyAttributes.HasDefault, type, null);           
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;          
            MethodBuilder getMethodBuilder = myTypeBuilder.DefineMethod("get_" + name, getSetAttr, type, Type.EmptyTypes);         
            ILGenerator custNameGetIL = getMethodBuilder.GetILGenerator();       
            custNameGetIL.Emit(OpCodes.Ldarg_0);        
            custNameGetIL.Emit(OpCodes.Ldfld, myFieldBuilder);          
            custNameGetIL.Emit(OpCodes.Ret);        
            MethodBuilder setMethodBuilder = myTypeBuilder.DefineMethod("set_" + name, getSetAttr, null, new Type[] { type });          
            ILGenerator custNameSetIL = setMethodBuilder.GetILGenerator();       
            custNameSetIL.Emit(OpCodes.Ldarg_0);          
            custNameSetIL.Emit(OpCodes.Ldarg_1);        
            custNameSetIL.Emit(OpCodes.Stfld, myFieldBuilder);       
            custNameSetIL.Emit(OpCodes.Ret);         
            myPropertyBuilder.SetGetMethod(getMethodBuilder);        
            myPropertyBuilder.SetSetMethod(setMethodBuilder);      
        }   
    }

    public class DataRowCollection : List<DataRow> { }

    public class DataTable    
    {      
        /// <summary>  
        /// /// DataTable 的名字       
        /// /// </summary>      
        public string Name { get; set; }  
        /// <summary>       
        /// /// DataRow的集合   
        /// /// </summary>     
        public DataRowCollection Rows { get; set; }  
        /// <summary>    
        /// DataColumn的集合      
        /// </summary>    
        public DataColumnCollection  Columns  { get; set; }   
        /// <summary>    
        /// 构造函数并且赋初始值或创建对象        
        /// </summary>        
        /// <param name="name"></param>   
        public DataTable(string name )       
        {          
            this.Name = name;     
            this.Rows = new DataRowCollection();   
            this.Columns = new DataColumnCollection(); 
        }  
    }

    /// <summary>  
    /// 自定义MyDataGrid 继承自DataGrid    
    /// </summary>   
    public class MyDataGrid:DataGrid   
    {      
        public MyDataGrid()     
        {          
            //重新定义触发AutoGeneratingColumn时的创建列的方法      
            this.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>           
                (            
                (o, e) =>                  
                {                     
                    //将dataSource赋给自定义的Datatable             
                    DataTable dt = ((DataTable)this.DataSoruce);             
                    //通过自定义DataColumn设置对dataGrid的Cloumn进行相应的修改    
                    foreach (DataColumn dc in dt.Columns)              
                    {                         
                        if (dc.ColumnName.ToUpper() == e.Column.Header.ToString())  
                        {                       
                            e.Column.Header = dc.Caption;          
                            e.Column.IsReadOnly = dc.ReadOnly;   
                            e.Column.CanUserResize = dc.AllowResize;  
                            e.Column.CanUserSort = dc.AllowSort;  
                            e.Column.CanUserReorder = dc.AllowReorder;   
                            break;                    
                        }            
                    }                                
                }          
                );      
        }       
        public object DataSoruce { set; get; }       
        /// <summary>       
        /// 将DataTable转换成list<object>    
        /// </summary>       
        /// <param name="table">自定义DataTable</param>     
        /// <returns></returns>      
        public List<object> GetDataFromDataTable(DataTable  table )       
        {          
            List<object> list = new List<object>();   
            foreach (DataRow row in table.Rows)         
            {          
                Assembly rowAssembly = row.EmitAssembly();   
                object c=rowAssembly.CreateInstance("DataRowObject");      
                Type type = rowAssembly.GetType("DataRowObject");        
                foreach (string key in row.items.Keys)           
                {                 
                    PropertyInfo properInfo = type.GetProperty(key.ToUpper());  
                    properInfo.SetValue(c, row.items[key], null);        
                }               
                list.Add(c);          
            }           
            return list;         
        }      
        public void DataBind()   
        {           
            this.ItemsSource = this.GetDataFromDataTable((DataTable)DataSoruce); 
        }   
    }

}
