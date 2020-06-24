using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MusicSorter
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			this.Height = 230;      //длина формы
		}

		//класс одной песни
		public class Song
		{
			public string Full;     //полная строка пути
			public string Path;     //путь к файлу
			public string File;     //файл
			public string Singer;   //исполнитель основной
			public string Singers;  //исполнитель
			public string Title;    //название
			public string Format;   //формат

			private string[] Separators = {", ", " vs. ", " feat. ", " & "}; 

			//конструктор обработки строки
			public Song(string str)
			{
				int indSlash = str.LastIndexOf('\\');
				int indDash = str.LastIndexOf(" - ");
				int indPoint = str.LastIndexOf(".");
				Full = str;
				Path = str.Substring(0, indSlash);
				File = str.Substring(indSlash + 1);
				Singer = ""; //что бы проверить, что исполнитель всего один
				Singers = str.Substring(indSlash + 1, indDash - indSlash - 1); //если исполнителей много
				foreach (string Sep in Separators) //поиск разделителя
				{
					int indSep = Singers.IndexOf(Sep);
					if (indSep != -1)			//если есть, берём первого исполнителя
					{
						Singer = Singers.Substring(0, indSep); //первый до разделителя
						break;
					}
				}
				if (Singer == "") Singer = Singers; //если исполнитель всего один
				Title = str.Substring(indDash + 3, indPoint - indDash - 3);
				Format = str.Substring(indPoint);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//получение пути сортируемой папки
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				//пусть сортировки
				textBox1.Text = folderBrowserDialog1.SelectedPath;
				if (textBox1.Text.Length > 3) //если не корень диска
				{
					//индекс последнего \ , для возвращения по каталогу вверх
					int ind = textBox1.Text.LastIndexOf('\\');
					if (ind == 2) //если папка сразу после корня
					{
						//добавить слеш
						textBox2.Text = textBox1.Text.Remove(ind) + '\\';
					}
					else
					{
						//иначе не добавлять
						textBox2.Text = textBox1.Text.Remove(ind);
					}
				}
				else
				{
					//если корень диска, то пути одинаковы
					textBox2.Text = textBox1.Text;
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			//получение пути основной папки
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				textBox2.Text = folderBrowserDialog1.SelectedPath;
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			string[] Catalog;                       //массив путей файлов
			List<Song> Songs = new List<Song>();    //лист песен
			int CountSort = 0;                      //минимальное число сортируемых
			int InfDir = 0, InfSong = 0;            //информация о сортировке
			string SortPath = textBox1.Text;        //путь сортировки в переменную
			string MainPath = textBox2.Text;        //основной путь в переменную
			listBox1.Items.Clear();                 //чистка листа папок
			listBox2.Items.Clear();                 //чистка листа песен
			string PathPattern = @"^\D:\.*";        //шаблон начала пути (рег. выражение: "[Символ]:\")

			try             //отлов исключения ошибки ввода CountSort
			{ CountSort = Convert.ToInt32(textBox3.Text); } //взятие минимального количества песен
			catch
			{
				MessageBox.Show("Минимальное количество сортируемых песен указано неверно!\nВведите целое число.",
					"Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				return;
			}

			try             //отлов исключения ошибки директории сортировки
			{ Catalog = Directory.GetFiles(SortPath); } //взять все файлы из каталога с путями
			catch
			{
				MessageBox.Show("Сортируемая папка не выбрана, или путь к ней указан неверно", "Ошибка!",
					MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				return;
			}
			//предупреждение о возможном неправильном пути основной деректории
			if (!System.Text.RegularExpressions.Regex.IsMatch(MainPath, PathPattern))
			{
				DialogResult result = MessageBox.Show(
					"Основной путь не сответствует стандарту представления пути. Каталоги будут созданы в неопределённом месте! Продолжить?",
					"Предупреждение!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
				if (result == DialogResult.No)
					return;
			}

			//заполнение листа песен
			foreach (string S in Catalog)
			{
				Songs.Add(new Song(S));
			}
			//групировка по исполнителю
			var SingerGroups = Songs.GroupBy(s => s.Singer);
			//проходим по группам исполнителей
			foreach (var Singer in SingerGroups)
			{
				//если песен у группы больше или равно минимального количества
				if (Singer.Count() >= CountSort)
				{
					//создаём папку, если не создана и не создаём, если есть
					DirectoryInfo dir = new DirectoryInfo(MainPath + '\\' + Singer.Key);
					if (!dir.Exists)
					{
						dir.Create();
						InfDir++; //подсчёт созданых папок
						listBox1.Items.Add(dir.Name); //вывод созданных папок
					}
					//проходим по песням группы
					foreach (Song song in Singer)
					{
						//удаление файла, если он существует (для замены файлов)
						File.Delete(dir.FullName + '\\' + song.File);
						//перемещаем песни в папку
						File.Move(song.Full, dir.FullName + '\\' + song.File);
						InfSong++; //подсчёт перемещённых треков
						listBox2.Items.Add(song.File); //вывод перемещённых треков
					}
				}
			}

			//вывод сообщения об успехе и параметры
			MessageBox.Show("Сортировка выполнена!\nБыло создано " + InfDir + " каталогов.\nПеремещено "
				+ InfSong + " треков.", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information,
				MessageBoxDefaultButton.Button1);
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (this.Height == 230) //увеличить форму
			{
				this.Height = 400;
				button4.Text = "Скрыть";
			}
			else //уменьшить форму
			{
				this.Height = 230;
				button4.Text = "Информация о последней сортировке";
			}
		}
	}
}
