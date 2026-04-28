using System.IO;
using System.Text;
using System.Text.RegularExpressions;

var path = ""MVC/Views/BarAdmin/Index.cshtml"";
var text = File.ReadAllText(path, Encoding.UTF8);

text = text.Replace(""<th>Статус</th>"", ""<th>Статус</th>\r\n                    <th>ії</th>"");

text = text.Replace(""item.Category == \""Снеки\"""", ""item.Category == \""Snacks\"" || item.Category == \""Снеки\"""");
text = text.Replace(""item.Category == \""апої\"""", ""item.Category == \""Drinks\"" || item.Category == \""апої\"""");

var editHtml = @""
                            <a href=""""/BarAdmin/Edit/@item.Id"""" class=""""cyber-btn-sm"""" style=""""text-decoration:none; background: #FFD21E; color: #0B1222; margin-left: 10px; font-weight: 600; padding: 10px 15px; border-radius: 6px; display:inline-block; margin-top:5px;"""">
                                &#9998; едагувати
                            </a>
                        </td>
"";
text = Regex.Replace(text, @""</select>\s*</form>\s*</td>"", ""</select>\n                            </form>\n"" + editHtml);

File.WriteAllText(path, text, Encoding.UTF8);
