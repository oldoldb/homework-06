/**
 *  Go Applet
 *  1996.11		xinz	written in Java
 *  2001.3		xinz	port to C#
 *  2001.5.10	xinz	file parsing, back/forward
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Resources;
[assembly: CLSCompliant(true)]
namespace DesignLibrary { }

namespace GoWinApp
{

	public enum StoneColor : int //黑子白子
	{
		Black = 0, White = 1
	}


	/**
	 * 呵呵呵
	 */
	public class GoBoard : System.Windows.Forms.Form
	{
		string [] strLabels; // {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"};

		int nSize;		                //每行每列方格数 19
		const int nBoardMargin = 10;	//边线距离 10
		int nCoodStart = 4;
		const int	nBoardOffset = 20; //棋盘与左边距离 20
		int nEdgeLen = nBoardOffset + nBoardMargin; //棋盘右下角的横纵长度
		int nTotalGridWidth = 360 + 36;	//方格总大小
		int nUnitGridWidth = 22;		//每个小方格的大小
		int nSeq = 0;
		Rectangle rGrid;		    // rectangle for 整个棋盘
		StoneColor m_colorToPlay;   // 接下来要走的旗子颜色
		GoMove m_gmLastMove;	    // 上一步动作
		Boolean bDrawMark;	        // 是否highlight高亮显示(话说高亮做的好挫..)
		Boolean m_fAnyKill;	        // 是否有吃子动作
		Spot [,] Grid;		        // 记录棋盘状态的二维数组
		Pen penGrid; //各种色笔用来画图
        //被删掉了, penStoneW, penStoneB,penMarkW, penMarkB
		Brush brStar, brBoard, brBlack, brWhite, m_brMark; //各种画刷用来画图
	
        // >>Button <<Button
        int nFFMove = 10;   // 限制>>Button的最大操作数
  //      int nRewindMove = 10;  // 限制<<Button的最大操作数,但1.这个变量没有使用过 2.<<Button click的时候直接就reset了,所以根本没用

		GoTree	gameTree; //游戏逻辑部分

		///    各种UI变量声明
//		private System.ComponentModel.Container components;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button Rewind;
		private System.Windows.Forms.Button FForward;
		private System.Windows.Forms.Button Save;
		private System.Windows.Forms.Button Open;
		private System.Windows.Forms.Button Back;
		private System.Windows.Forms.Button Forward;

		public GoBoard(int nSize)
		{
			//
			// Form第一步,初始化各种组件
			//
			InitializeComponent();

			//
			// 各种初始化操作
			//

			this.nSize = nSize;  //设定棋盘大小

			m_colorToPlay = StoneColor.Black; //执黑先行

			Grid = new Spot[nSize,nSize]; //new一个Grid存状态
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					Grid[i,j] = new Spot();
            /*------以下各种初始化画笔画刷------*/
			penGrid = new Pen(Color.Brown, (float)0.5);
            //penStoneW = new Pen(Color.WhiteSmoke, (float)1);
            //penStoneB = new Pen(Color.Black,(float)1);
            //penMarkW = new Pen(Color.Blue, (float) 1);
            //penMarkB = new Pen(Color.Beige, (float) 1);

			brStar = new SolidBrush(Color.Black);
			brBoard = new SolidBrush(Color.Orange);
			brBlack = new SolidBrush(Color.Black);
			brWhite = new SolidBrush(Color.White);
			m_brMark = new SolidBrush(Color.Red);

			rGrid = new Rectangle(nEdgeLen, nEdgeLen,nTotalGridWidth, nTotalGridWidth);
			strLabels = new string [] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"};
			gameTree = new GoTree();
		}
        /*------绘制UI------*/
        #region
		///    动态绘制各种组件,没什么好说的
		///    
        private void InitializeComponent()
		{
            this.Open = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.Rewind = new System.Windows.Forms.Button();
            this.Forward = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.FForward = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Open
            // 
            this.Open.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Open.Location = new System.Drawing.Point(534, 95);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(67, 25);
            this.Open.TabIndex = 2;
            this.Open.Text = "open";
            this.Open.Click += new System.EventHandler(this.OpenClick);
            // 
            // Save
            // 
            this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Save.Location = new System.Drawing.Point(611, 95);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(67, 25);
            this.Save.TabIndex = 3;
            this.Save.Text = "save";
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // Rewind
            // 
            this.Rewind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Rewind.Location = new System.Drawing.Point(611, 60);
            this.Rewind.Name = "Rewind";
            this.Rewind.Size = new System.Drawing.Size(67, 25);
            this.Rewind.TabIndex = 5;
            this.Rewind.Text = "<<";
            this.Rewind.Click += new System.EventHandler(this.RewindClick);
            // 
            // Forward
            // 
            this.Forward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Forward.Location = new System.Drawing.Point(534, 26);
            this.Forward.Name = "Forward";
            this.Forward.Size = new System.Drawing.Size(67, 25);
            this.Forward.TabIndex = 0;
            this.Forward.Text = ">";
            this.Forward.Click += new System.EventHandler(this.ForwardClick);
            // 
            // Back
            // 
            this.Back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Back.Location = new System.Drawing.Point(611, 26);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(67, 25);
            this.Back.TabIndex = 1;
            this.Back.Text = "<";
            this.Back.Click += new System.EventHandler(this.BackClick);
            // 
            // FForward
            // 
            this.FForward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FForward.Location = new System.Drawing.Point(534, 60);
            this.FForward.Name = "FForward";
            this.FForward.Size = new System.Drawing.Size(67, 25);
            this.FForward.TabIndex = 4;
            this.FForward.Text = ">>";
            this.FForward.Click += new System.EventHandler(this.FForwardClick);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(536, 138);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(144, 335);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "please open a .sgf file to view, or just play on the board";
            // 
            // GoBoard
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Rewind);
            this.Controls.Add(this.FForward);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Open);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Forward);
            this.Name = "GoBoard";
            this.Text = "Go_WinForm";
            this.Click += new System.EventHandler(this.GoBoardClick);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintHandler);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
        #endregion

		private void PaintHandler(Object sender, PaintEventArgs e)
		{
			UpdateGoBoard(e); //更新棋盘
		}
        /*------Save功能都没有实现?------*/
        protected void SaveClick(object sender, System.EventArgs e)
        {
            SaveFile();
            return;
        }
        /*------话说这些简单的button响应事件就不用多说了吧?------*/
        protected void OpenClick(object sender, System.EventArgs e)
        {
            OpenFile();
            ShowGameInfo();//棋谱文件可能带有自述信息
        }
        #region
        protected void RewindClick(object sender, System.EventArgs e)
        {
            gameTree.Reset();//<<Button需要完成游戏逻辑重置,棋盘重置,并重新显示游戏信息
            ResetBoard();
            ShowGameInfo();
        }

        protected void FForwardClick(object sender, System.EventArgs e)
        {
            if (gameTree != null)
            {
                int i = 0;
                GoMove gm = null;
                for (gm = gameTree.DoNext(); gm != null; gm = gameTree.DoNext())
                {
                    PlayNext(ref gm);
                    if (i++ > nFFMove)//将棋盘状态恢复到允许恢复到的最新状态
                        break;
                }
            }
        }

        protected void ForwardClick(object sender, System.EventArgs e)
        {
            GoMove gm = gameTree.DoNext();
            if (null != gm)//前进到有历史记录的下一个操作
            {
                PlayNext(ref gm);
            }
        }
		private void ShowGameInfo()
		{
			//显示游戏信息
			textBox1.Clear();
			textBox1.AppendText(gameTree.Info);
		}

        protected void BackClick(object sender, System.EventArgs e)
        {
            GoMove gm = gameTree.DoPrev();	//游戏历史记录中的前一步
            if (null != gm)
            {
                PlayPrev(gm);
            }
            else
            {
                ResetBoard();
                ShowGameInfo();
            }
        }

		Boolean OnBoard(int x, int y) //边界处理
		{
			return (x>=0 && x<nSize && y>=0 && y<nSize);
		}
        /*------又在没用可删的范畴内------*/
        protected void GoBoardClick(object sender, System.EventArgs e)
        {
            return;
        }
        /*------将坐标转换为棋盘中的图块------*/
		private Point PointToGrid(int x, int y)
		{
			Point p= new Point(0,0);
			p.X = (x - rGrid.X + nUnitGridWidth/2) / nUnitGridWidth;
			p.Y = (y - rGrid.Y + nUnitGridWidth/2) / nUnitGridWidth;
			return p;
		}

		//设定了在相交点附近怎样的范围内松开鼠标就视为在此处落子
		//
		private Boolean CloseEnough(Point p, int x, int y)
		{
			if (x < rGrid.X+nUnitGridWidth*p.X-nUnitGridWidth/3 ||
				x > rGrid.X+nUnitGridWidth*p.X+nUnitGridWidth/3 ||
				y < rGrid.Y+nUnitGridWidth*p.Y-nUnitGridWidth/3 ||
				y > rGrid.Y+nUnitGridWidth*p.Y+nUnitGridWidth/3)
			{
				return false;
			}
			else 
				return true;
		}
        /// 鼠标松开事件,用来处理落子
		private void MouseUpHandler(Object sender,MouseEventArgs e)
		{
			Point p;
			GoMove	gmThisMove;

			p = PointToGrid(e.X,e.Y);
			if (!OnBoard(p.X, p.Y) || !CloseEnough(p,e.X, e.Y)|| Grid[p.X,p.Y].HasStone())
				return; //不满足落子条件

			//满足落子条件时,落子,并将thismove传到gametree中
			gmThisMove = new GoMove(p.X, p.Y, m_colorToPlay, 0);
			PlayNext(ref gmThisMove);
			gameTree.AddMove(gmThisMove);
		}

        public void PlayNext(ref GoMove gm) 
		{
            if (gm == null)
            {
                throw new ArgumentNullException("gm");
            }
			Point p = gm.Point; //获得当前move点
			m_colorToPlay = gm.Color;	//接下来要下子的颜色

			//清除highlight信息和落子信息
			ClearLabelsAndMarksOnBoard();

            if (m_gmLastMove != null)//如果上一轮已经落子,那么取消该棋子的高亮显示
            {
                RepaintOneSpotNow(m_gmLastMove.Point);
            }
			bDrawMark = true; //高亮显示
			Grid[p.X,p.Y].SetStone(gm.Color); //落子
			m_gmLastMove = new GoMove(p.X, p.Y, gm.Color, nSeq++); //将本次操作记录在lastmove中
			//棋盘显示所有的labels和mark
			SetLabelsOnBoard(gm);
			SetMarksOnBoard(gm);
			
			DoDeadGroup(NextTurn(m_colorToPlay));//nextturn返回下一轮的行子颜色，这一操作即进行吃子动作,注意先判定本轮落子能否先吃对方
			//如果有吃子的动作，那么就存入deadgroup中 
            if (m_fAnyKill)
            {
                AppendDeadGroup(ref gm, NextTurn(m_colorToPlay));
            }
            else //否则要判是否会被吃
            {
                DoDeadGroup(m_colorToPlay);
                if (m_fAnyKill)
                {
                    AppendDeadGroup(ref gm, m_colorToPlay);
                }
            }
			m_fAnyKill = false;
			
			OptRepaint();//重绘棋盘

			//更新下一轮落子颜色
			m_colorToPlay = NextTurn(m_colorToPlay);
			
			//......更新游戏信息,这里其实可以重写showgameinfo()
			textBox1.Clear();
			textBox1.AppendText(gm.Comment);
		}
        /*------添加被吃的颜色为c的子------*/
		private void AppendDeadGroup(ref GoMove gm, StoneColor c)
		{
			ArrayList a = new ArrayList();
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].IsKilled())
					{
						Point pt = new Point(i,j);
						a.Add(pt);
						Grid[i,j].SetNoKilled();//这里...真的有必要封装成这个样子么..
					}
			gm.DeadGroup = a;//存入本次动作gm中,gm.deadgroup就存放了本轮被吃的子,于是playprev可以用到
			gm.DeadGroupColor = c;
		}
        /*------重绘棋盘------*/
		public void ResetBoard()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].RemoveStone();
			m_gmLastMove = null;
			Invalidate(null);
		}

		/*
		 * play the move so that the game situation is just BEFORE this move is played.
		 * what to do:
		 * 	1. remove the current move from the board :removestone
		 *  1.1 also remove the "lastmov" highlight :bDrawMark=false;
		 *	2. store the stones got killed by current move
		 *  3. highlight the new "lastmove" :bDrawMark=true
		 */
		public void PlayPrev(GoMove gm)
		{
            if (gm == null)
            {
                throw new ArgumentNullException("gm");
            }
            Point p = gm.Point;//获得当前要移除的点
            m_colorToPlay = gm.Color;//要移除的点的颜色
            ClearLabelsAndMarksOnBoard();//清除highlight信息和落子信息 
            Grid[p.X, p.Y].RemoveStone();//remove the current move from the board
            bDrawMark = false;//also remove the "lastmove" highlight
            RepaintOneSpotNow(p); 
            if (gm.DeadGroup != null)//恢复被吃掉的子
            {
                for (int i = 0; i <gm.DeadGroup.Count; i++)
                {
                    Point pp = (Point)gm.DeadGroup[i];
                    RepaintOneSpotNow(pp);
                    Grid[pp.X, pp.Y].SetStone(gm.DeadGroupColor);
                }

            }
            m_gmLastMove = gameTree.PeekPrev();
            if (m_gmLastMove != null)//highlight the new "lastmove"
            {
                bDrawMark = true;
                RepaintOneSpotNow(m_gmLastMove.Point);
                SetLabelsOnBoard(m_gmLastMove);
                SetMarksOnBoard(m_gmLastMove);
            }
            OptRepaint();
        }

				
		
		Rectangle GetUpdatedArea(int i, int j) //返回需要更新重绘的区域
		{
			int x = rGrid.X + i * nUnitGridWidth - nUnitGridWidth/2;
			int y = rGrid.Y + j * nUnitGridWidth - nUnitGridWidth/2;
			return new Rectangle(x,y, nUnitGridWidth, nUnitGridWidth);
		}

		/**
		 * 重绘
		 */
        private void OptRepaint()
		{
			Rectangle r = new Rectangle(0,0,0,0);
			Region	re;

			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].IsUpdated()) 
					{
						r = GetUpdatedArea(i,j);
						re = new Region(r);
						Invalidate(re);
                        re.Dispose();
					}
		}

		/*
		 * 只重回一个交叉点,用在本轮已经落子需要进行>>或者<<操作的时候
		 */
        public void RepaintOneSpotNow(Point point)
		{
            Grid[point.X, point.Y].SetUpdated();
			bDrawMark = false;
            Rectangle r = GetUpdatedArea(point.X, point.Y);
            Region re=new Region(r);
			Invalidate(re);
            re.Dispose();
            Grid[point.X, point.Y].ResetUpdated();
			bDrawMark = true;
		}

		//字面意思是记录操作，但是这个函数没有用到过，很可疑 
        public void RecordMove(Point point, StoneColor colorToPlay) 
		{
            Grid[point.X, point.Y].SetStone(colorToPlay);
			// 将上一个操作置为该次操作
            m_gmLastMove = new GoMove(point.X, point.Y, colorToPlay, nSeq++);
		}
        //返回下次落子的颜色
		static StoneColor NextTurn(StoneColor c) 
		{
			if (c == StoneColor.Black)
				return StoneColor.White;
			else 
				return StoneColor.Black;
		}

		/**
		 *	bury the dead stones in a group (same color). 
		 *	if a stone in one group is dead, the whole group is dead.
         *	说白了就是dfs消除相连的气为0的子
		*/
		void BuryTheDead(int i, int j, StoneColor c) 
		{
			if (OnBoard(i,j) && Grid[i,j].HasStone() && 
				Grid[i,j].Color() == c) 
			{
				Grid[i,j].Die();
				BuryTheDead(i-1, j, c);
				BuryTheDead(i+1, j, c);
				BuryTheDead(i, j-1, c);
				BuryTheDead(i, j+1, c);
			}
		}
        /*------清除扫描记录,就是清除visit数组的意思------*/
		void CleanScanStatus()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].ClearScanned();
		}

		/**
		 * 扫描整个棋盘,对当前颜色c,提掉所有气为0的子
		 */
		void DoDeadGroup(StoneColor c) 
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					if (Grid[i,j].HasStone() &&
						Grid[i,j].Color() == c) 
					{
						if (CalcLiberty(i,j,c) == 0)
						{
							BuryTheDead(i,j,c);
							m_fAnyKill = true;
						}
						CleanScanStatus();
					}
		}
        /*------非递归BFS实现计算气------*/
        /*
        int CalcLibertyBfs(int x, int y, StoneColor c)
        {
            int lib=0;
            int[] dx = { 1, 0, -1, 0 };
            int[] dy = { 0, -1, 0, 1 };
            Queue q = new Queue();
            Point s=new Point(x,y);
            Point next = new Point();
            q.Enqueue(s);
            while (q.Count > 0)
            {
                s = (Point)q.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    next.X = s.X + dx[i];
                    next.Y = s.Y + dy[i];
                    if (!OnBoard(next.X, next.Y))
                    {
                        continue;
                    }
                    if (Grid[next.X, next.Y].IsScanned())
                    {
                        continue;
                    }
                    if (!Grid[next.X, next.Y].HasStone())
                    {
                        lib++;
                    }
                    if (Grid[next.X, next.Y].Color() == c)
                    {
                        q.Enqueue(next);
                    }
                    Grid[next.X, next.Y].SetScanned();
                }
            }
            return lib;

        }
        */
		/**
		 * dfs计算每个子(每个group)的气
		 */
		int CalcLiberty(int x, int y, StoneColor c) 
		{
			int lib = 0; // 初始化
			
			if (!OnBoard(x,y))
				return 0;
			if (Grid[x,y].IsScanned())
				return 0;

			if (Grid[x,y].HasStone()) 
			{
				if (Grid[x,y].Color() == c) 
				{
					//dfs深搜四个相邻的格子
					Grid[x,y].SetScanned();
					lib += CalcLiberty(x-1, y, c);
					lib += CalcLiberty(x+1, y, c);
					lib += CalcLiberty(x, y-1, c);
					lib += CalcLiberty(x, y+1, c);
				} 
				else 
					return 0;
			} 
			else 
			{// 周围没有棋子的话气+1
				lib ++;
				Grid[x,y].SetScanned();
			}

			return lib;
		}


		/**
		 * 高亮显示上一次的落子
		 */
		void MarkLastMove(Graphics g) 
		{
			Brush brMark;
			if (m_gmLastMove.Color == StoneColor.White)
				brMark = brBlack;
			else 
				brMark = brWhite;
			Point p = m_gmLastMove.Point;
			g.FillRectangle( brMark,
				rGrid.X + (p.X) * nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + (p.Y) * nUnitGridWidth - (nUnitGridWidth-1)/8,
				3, 3);
		}
        /*------消除所有棋子和高亮标记------*/
		private void ClearLabelsAndMarksOnBoard()
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].HasLabel())
						Grid[i,j].ResetLabel();
					if (Grid[i,j].HasMark())
						Grid[i,j].ResetMark();
				}

		}
        /*------就是按照当前操作gm,将gm之前的labels全部落下------*/
		private void SetLabelsOnBoard(GoMove gm)
		{
			short	nLabel = 0;
			Point p;
			if (null != gm.Labels)
			{
				System.Collections.IEnumerator myEnumerator = gm.Labels.GetEnumerator();
				while (myEnumerator.MoveNext())
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].SetLabel(++nLabel);
				}
			}
		}
        /*------设置棋盘上的高亮显示------*/
		private void SetMarksOnBoard(GoMove gm)
		{
			Point p;
			if (null != gm.Labels)
			{
				System.Collections.IEnumerator myEnumerator = gm.Marks.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].SetMark();
				}
			}
		}
        
		static private Point SwapXY(Point p)//额...交换两点坐标
		{
			Point pNew = new Point(p.Y,p.X);
			return pNew;
		}
        /*------绘制棋盘------*/
        private void DrawBoard(Graphics g)
		{
			//绘制坐标
			string[] strV= {"1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19"};
			string [] strH= {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"};

			Point p1 = new Point(nEdgeLen,nEdgeLen);
			Point p2 = new Point(nTotalGridWidth+nEdgeLen,nEdgeLen);
			g.FillRectangle(brBoard,nBoardOffset,nBoardOffset,nTotalGridWidth+nBoardOffset,nTotalGridWidth+nBoardOffset);
			for (int i=0;i<nSize; i++)
			{
				g.DrawString(strV[i],this.Font, brBlack, 0, nCoodStart+ nBoardOffset + nUnitGridWidth*i);
				g.DrawString(strH[i],this.Font, brBlack, nBoardOffset + nCoodStart + nUnitGridWidth*i, 0);
				g.DrawLine(penGrid, p1, p2);
				g.DrawLine(penGrid, SwapXY(p1), SwapXY(p2));

				p1.Y += nUnitGridWidth;
				p2.Y += nUnitGridWidth;
			}
			//绘制边线
			Pen penHi = new Pen(Color.WhiteSmoke, (float)0.5);
			Pen penLow = new Pen(Color.Gray, (float)0.5);

			g.DrawLine(penHi, nBoardOffset, nBoardOffset, nTotalGridWidth+2*nBoardOffset, nBoardOffset);
			g.DrawLine(penHi, nBoardOffset, nBoardOffset, nBoardOffset, nTotalGridWidth+2*nBoardOffset);
			g.DrawLine(penLow, nTotalGridWidth+2*nBoardOffset,nTotalGridWidth+2*nBoardOffset, nBoardOffset+1, nTotalGridWidth+2*nBoardOffset);
			g.DrawLine(penLow, nTotalGridWidth+2*nBoardOffset,nTotalGridWidth+2*nBoardOffset, nTotalGridWidth+2*nBoardOffset, nBoardOffset+1);
		}

		void UpdateGoBoard(PaintEventArgs e)
		{
			DrawBoard(e.Graphics);
			
			//绘制天元
			DrawStars(e.Graphics);

			//绘制每一个区域
			DrawEverySpot(e.Graphics);
		}

		//绘制天元
		void DrawStar(Graphics g, int row, int col) 
		{
			g.FillRectangle(brStar,
				rGrid.X + (row-1) * nUnitGridWidth - 1, 
				rGrid.Y + (col-1) * nUnitGridWidth - 1, 
				3, 
				3);
		}

		//绘制9个天元
		void  DrawStars(Graphics g)
		{
			DrawStar(g, 4, 4);
			DrawStar(g, 4, 10);
			DrawStar(g, 4, 16);
			DrawStar(g, 10, 4);
			DrawStar(g, 10, 10);
			DrawStar(g, 10, 16);
			DrawStar(g, 16, 4);
			DrawStar(g, 16, 10);
			DrawStar(g, 16, 16);
		}

		/**
		 * 绘制落子
		 */
		void DrawStone(Graphics g, int row, int col, StoneColor c) 
		{
			Brush br;
			if (c == StoneColor.White)
				br = brWhite;
			else 
				br = brBlack;
			
			Rectangle r = new Rectangle(rGrid.X+ (row) * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + (col) * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(br, r);
		}
        /*------这个函数貌似没有调用过?------*/
		void DrawLabel(Graphics g, int x, int y, short nLabel) 
		{
			if (nLabel ==0)
				return;
			nLabel --;
			nLabel %= 18;			//各种转换

			//画Label
			Rectangle r = new Rectangle(rGrid.X+ x * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(brBoard, r);
			g.DrawString(strLabels[nLabel],	//填字符串
				this.Font, 
				brBlack, 
				rGrid.X+ (x) * nUnitGridWidth - (nUnitGridWidth-1)/4, 
				rGrid.Y + (y) * nUnitGridWidth - (nUnitGridWidth-1)/2);
		}
        /*------绘制highlight点------*/
		void DrawMark(Graphics g, int x, int y)
		{
			g.FillRectangle( m_brMark,
				rGrid.X + x* nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/8,
				5, 5);
		}
        /*------绘制每一个落子点------*/
		void DrawEverySpot(Graphics g) 
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].HasStone())
						DrawStone(g, i, j, Grid[i,j].Color());
					if (Grid[i,j].HasLabel())
						DrawLabel(g, i, j, Grid[i,j].GetLabel());
					if (Grid[i,j].HasMark())
						DrawMark(g, i, j);
				}
			//标记操作
			if (bDrawMark && m_gmLastMove != null)
				MarkLastMove(g);
		}
        #endregion
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:不要多次释放对象"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:请不要将文本作为本地化参数传递", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:请不要将文本作为本地化参数传递", MessageId = "System.Windows.Forms.FileDialog.set_Filter(System.String)")]
        private void SaveFile()
        {
            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                saveDlg.Filter = "sgf files (*.sgf)|*.sgf|All Files (*.*)|*.*";
                saveDlg.DefaultExt = ".sgf";
                DialogResult res = saveDlg.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (!(saveDlg.FileName).EndsWith(".sgf") && !(saveDlg.FileName).EndsWith(".SGF"))
                        MessageBox.Show("Unexpected file format", "Super Go Format", MessageBoxButtons.OK);
                    else
                    {
                        using (StreamWriter w = new StreamWriter(saveDlg.FileName, false))
                        {
                            string s = gameTree.Info;//这里应该递归掉gm的信息,但是gm目前不是全局变量.
                            w.WriteLine(s);
                        }
                    }
                }
            }
        }
        //打开棋谱文件
        private void OpenFile()
		{
			OpenFileDialog openDlg = new OpenFileDialog();
			openDlg.Filter  = "sgf files (*.sgf)|*.sgf|All Files (*.*)|*.*";
			openDlg.FileName = "" ;
			openDlg.DefaultExt = ".sgf";
			openDlg.CheckFileExists = true;
			openDlg.CheckPathExists = true;
			
			DialogResult res = openDlg.ShowDialog ();
			
			if(res == DialogResult.OK)
			{
                if (!(openDlg.FileName).EndsWith(".sgf") && !(openDlg.FileName).EndsWith(".SGF"))
                    MessageBox.Show("Unexpected file format", "Super Go Format", MessageBoxButtons.OK);
                else
                {
                    FileStream f = new FileStream(openDlg.FileName, FileMode.Open);
                    StreamReader r = new StreamReader(f);
                    string s = r.ReadToEnd();
                    gameTree = new GoTree(s);
                    gameTree.Reset();
                    ResetBoard();
                //    r.Close();
                    f.Close();
                }
			}		
		}	
	}

	public class GoTest
	{
		/// 
		/// main入口,单线程标记
		/// 
        [STAThread]
		public static void Main(string[] args) 
		{
			Application.Run(new GoBoard(19));
		}
	}

	
	//Spot类代表每一个点
	public class Spot 
	{
		private Boolean bEmpty; //是否为空
		private Boolean bKilled; //是否被吃
		private Stone s; //落子属性
		private short	m_nLabel; 
		private Boolean m_bMark; //是否高亮
		private Boolean bScanned; //是否被扫描过
		private Boolean bUpdated; //是否已经被更新
		/**
		 * 构造函数
		 */
		public Spot() 
		{
			bEmpty = true;
			bScanned = false;
			bUpdated = false;
			bKilled = false;
		}
		/*------简单各种方法------*/
		public Boolean HasStone() { return !bEmpty;	}
		public Boolean IsEmpty() {	return bEmpty;	}
		public Stone ThisStone() {	return s;}
		public StoneColor Color() {	return s.color;}

		public Boolean HasLabel() {return m_nLabel>0;}
		public Boolean HasMark() {return m_bMark;}
		public void SetLabel(short label) {m_nLabel = label; bUpdated = true; }
		public void SetMark() {m_bMark = true; bUpdated = true;}
		public void ResetLabel() {m_nLabel = 0; bUpdated = true;}
		public void ResetMark() {m_bMark = false; bUpdated = true;}
        public short	GetLabel() {return m_nLabel;}

		public Boolean IsScanned() { return bScanned;}
		public void SetScanned() {	bScanned = true;}
		public void ClearScanned() { bScanned = false; }

		public void SetStone(StoneColor color) 
		{
			if (bEmpty) 
			{
				bEmpty = false;
				s.color = color;
				bUpdated = true;
			} // 落子
		}

		/*
		 * 提子
		*/
		public void RemoveStone()
		{	//提子
			bEmpty = true;
			bUpdated = true;
		}
				
		//被吃
		public void Die() 
		{
			bKilled = true;
			bEmpty = true;
			bUpdated = true;
		} 

		public Boolean IsKilled() { return bKilled;}
		public void SetNoKilled() { bKilled = false;}

		public void ResetUpdated() { bUpdated = false; bKilled = false;}

		//是否被更新
		public Boolean IsUpdated() 
		{ 
			if (bUpdated)
			{	//如果已经被更新了,那么置更新状态为反
				bUpdated = false;
				return true;
			} 
			else 
				return false;
		}

		// updated的set函数
		public void SetUpdated() { bUpdated = true; }
	}

	/**
	 * 操作类
	 */
	public class GoMove 
	{
		StoneColor m_c;	//落子颜色
		Point m_pos;		//落子位置
		int m_n;			//操作数
		String m_comment;	// 每步操作的信息
		MoveResult m_mr;	//每步操作的结果

		ArrayList		m_alLabel; //所有的label 
		ArrayList		m_alMark; //所有的mark

		//所有的被吃点
		//被吃子的颜色 
		ArrayList		m_alDead;
		StoneColor	m_cDead;
		/**
		 * 构造函数
		 */
		public GoMove(int posX, int posY, StoneColor sc, int seq) 
		{
			m_pos = new Point(posX,posY);
			m_c = sc;
			m_n = seq;
			m_mr = new MoveResult();
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}
        /*------这种构造函数用于棋谱文件------*/
		public GoMove(String str, StoneColor color) 
		{
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
			char cx = str[0];
			char cy = str[1];
			m_pos = new Point(0,0);
			//转换为坐标
			m_pos.X = (int) ( (int)cx - (int)(char)'a');
			m_pos.Y = (int) ( (int)cy - (int)(char)'a');
			this.m_c = color;
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}

        /*------还是将游戏信息中的str转换为点坐标,所以说上面的构造函数代码覆盖率太差------*/
		static private Point	StrToPoint(String str)
		{
			Point p = new Point(0,0);
			char cx = str[0];
			char cy = str[1];
			//转换为坐标
			p.X = (int) ( (int)cx - (int)(char)'a');
			p.Y = (int) ( (int)cy - (int)(char)'a');
			return p;
		}
        /*------各种get/set------*/
        //落子颜色
        public StoneColor Color
        { 
            get { return m_c; } 
        }
        //游戏信息
        public String Comment 
        {
            get
            {
                if (m_comment == null)
                    return string.Empty;
                else
                    return m_comment;
            }
            set
            {
                m_comment = value; 
            }
        }

		public int Seq
        {
            get { return m_n; }
            set {	m_n = value;}
        }

        public Point Point
        {
           get  { return m_pos; }
        }

        public MoveResult Result
        {
            get { return m_mr; }
            set { m_mr = value; }
        }

        public ArrayList DeadGroup
        {
            get { return m_alDead;}
            set {m_alDead = value;}
        }

        public StoneColor DeadGroupColor
        {
            get { return m_cDead; }
            set { m_cDead = value; }
        }
		
		public void AddLabel(String str) {m_alLabel.Add(StrToPoint(str));}
		
		public void AddMark(String str) {	m_alMark.Add(StrToPoint(str));}

        public ArrayList Labels
        {
            get { return m_alLabel; }
        }

        public ArrayList Marks
        {
            get { return m_alMark; }
        }
	}
	

	/**
	 * 操作结果类
	 * 
	 */
	public class MoveResult 
	{
        public StoneColor color; 
		// 4个方向是否被吃子 
        public Boolean bUpKilled;
        public Boolean bDownKilled;
        public Boolean bLeftKilled;
		public Boolean bRightKilled;
		public Boolean bSuicide;	//是否还有气
		public  MoveResult() 
		{
			bUpKilled = false;
			bDownKilled = false;
			bLeftKilled = false;
			bRightKilled = false;
			bSuicide = false;
		}
	}

	/**
	 * 
	 * ...这是在搞什么..突然又出现一个只有一个成员的struct..
	 */
	public struct Stone 
	{
		public StoneColor color; 
	}

	/**
	 * 操作变量类
	 * 
	 */
	public class GoVariation 
	{
	//	int m_id;			//用不到删掉了
	//	string m_name;	//用不到删掉了
		//请直接无视我 	//天知道你在说什么	
		ArrayList m_moves; 
		int m_seq;			//这个东西应该是在读谱的时候才有用的 
		int m_total;

		//构造函数
		public GoVariation(int id)
		{
		//	m_id = id;
			m_moves = new ArrayList(10);
			m_seq = 0;
			m_total = 0;
		}

		public void AddAMove(GoMove gm) 
		{
            if (gm == null)
            {
                throw new ArgumentNullException("gm");
            }
			gm.Seq = m_total ++;
			m_seq++;
			m_moves.Add(gm);
		}

		public void UpdateResult(GoMove gm) 
		{
		}

		public GoMove DoNext()
		{
			if (m_seq < m_total) 
			{
				return (GoMove)m_moves[m_seq++];
			} 
			else 
				return null;
		}

		public GoMove DoPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[--m_seq]);
			else 
				return null;
		}

		/*
		 *  嗯下面这个确实很有用，它返回操作队列的当前操作的前一个
		 */
		public GoMove PeekPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[m_seq-1]);
			else 
				return null;
		}

		public void Reset() {m_seq = 0;}
	}


	/**
	* 我想说下面的其实用不到 
	* 下面的用不到的 
	*/
    //struct VarStartPoint
    //{
    //    int m_id; 
    //    int m_seq;
    //}

	struct GameInfo //这个GameInfo其实有好多用不到的地方
	{
		public string gameName;
		public string playerBlack;
		public string playerWhite;
		public string rankBlack;
		public string rankWhite;
		public string result;
		public string date;
		public string km;
		public string size;
		public string comment;
        public string handicap;
        public string gameEvent;
        public string location;
        public string time;             // 时间
        public string unknown_ff;   //谁能告诉我这是什么...
        public string unknown_gm;
        public string unknown_vw; 
	}

    /*------这一坨应该是解析棋谱类------*/

	public class KeyValuePair 
	{
		public string k; public ArrayList alV;

		static private string	RemoveBackSlash(string strIn)
		{
			string strOut; 
			int		iSlash;

			strOut = string.Copy(strIn);
			if (strOut.Length < 2)
				return strOut;
			for (iSlash = strOut.Length-2; iSlash>=0; iSlash--)
			{
				if (strOut[iSlash] == '\\')		// && 转义字符首先，搜字符\
				{
					strOut = strOut.Remove(iSlash,1);
					if (iSlash>0)
						iSlash --;	// 就是专门解析棋谱文件的字符串操作...我们没必要花时间在这个上面
				}
			}
			return strOut;
		}
		public KeyValuePair(string k, string v)
		{
            if (k == null)
            {
                throw new ArgumentNullException("k");
            }
			this.k = string.Copy(k);
			string strOneVal;
			int		iBegin, iEnd;

            // 将棋谱信息转换为X[]的形式
			alV = new ArrayList(1);

            // Comment
			if (k.Equals("C"))
			{
				strOneVal = RemoveBackSlash(string.Copy(v));
                // Comment
				alV.Add(strOneVal);
				return;
			}
            if (v == null)
            {
                throw new ArgumentNullException("v");
            }
			iBegin = v.IndexOf("[");
            if (iBegin == -1)	// 里面是坐标
			{
				alV.Add(v);
				return; 
			}
			
			iBegin = 0;
			while (iBegin < v.Length && iBegin>=0)
			{
				iEnd = v.IndexOf("]", iBegin);
				if (iEnd > 0)
					strOneVal = v.Substring(iBegin, iEnd-iBegin);
				else
                    strOneVal = v.Substring(iBegin);	// 里面是坐标
				alV.Add(strOneVal);
				iBegin = v.IndexOf("[", iBegin+1);
				if (iBegin > 0)
                    iBegin++;	// 循环继续
			}
		}
	}

	//GoTree其实是操作队列

	public class GoTree 
	{
		GameInfo _gi;		//_gi:GameInfo游戏信息
		ArrayList _vars;		//forward的时候用
		int _currVarId;		//当前操作ID
//		int _currVarNum;
		GoVariation _currVar;		//当前操作
		string	_stGameComment;
       
		// 由棋谱产生的构造函数
		public GoTree(string str)
		{
			_vars = new ArrayList(10);
//			_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
			ParseFile(str); // 棋谱信息转换
		}

		//	平时用的构造函数
		public GoTree()
		{
			_vars = new ArrayList(10);
	//		_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
		}

		public	string Info
		{
            get
            {
                return _gi.comment == null? string.Empty : _gi.comment;
            }
		}

		public void AddMove(GoMove gm) 
		{
			_currVar.AddAMove(gm);
		}

		/**
		 * 顾名思义,棋谱文件转换用的
		 */
		Boolean ParseFile(String goStr) 
		{
			int iBeg, iEnd=0; 
			while (iEnd < goStr.Length) 
			{
				if (iEnd > 0)
					iBeg = iEnd;
				else 
					iBeg = goStr.IndexOf(";", iEnd);//从iEnd后第一个;的位置
				iEnd = goStr.IndexOf(";", iBeg+1);//indexof如果未找到会返回-1
                if (iEnd < 0) // 没找到
                    iEnd = goStr.LastIndexOf(")", goStr.Length);		//// 找最后一个）
				if (iBeg >= 0 && iEnd > iBeg) 
				{
					string section = goStr.Substring(iBeg+1, iEnd-iBeg-1);
					ParseASection(section);//划分出棋谱主体部分开始搞
				} 
				else 
					break;
			}
			return true;
		}

        /// 相当于词法分析提取一个单词
        static public int FindEndofValueStr(String sec)
        {
            if (sec == null)
            {
                throw new ArgumentNullException("sec");
            }
            int i = 0;
            // 截到]
            while (i >= 0)
            {
                i = sec.IndexOf(']', i+1);
                if (i > 0 && sec[i - 1] != '\\')
                    return i;    // 返回单词位置
            }

            // 没提到单词就返回总长度
            return sec.Length - 1;		// 没提到单词就返回总长度
        }
        
        static public int FindEndofValueStrOld(String sec)//老版,用不到
		{
            if (sec == null)
            {
                throw new ArgumentNullException("sec");
            }
			int i = 0;
            // 就是专门解析棋谱文件的字符串操作...我们没必要花时间在这个上面
			bool fOutside = false;
			
			for (i=0; i<sec.Length;i++)
			{
				if (sec[i] == ']')
				{
                    if (i > 1 && sec[i - 1] != '\\') // 就是专门解析棋谱文件的字符串操作...我们没必要花时间在这个上面
						fOutside = true;
				}
				else if (char.IsLetter(sec[i]) && fOutside && i>0)
					return i-1;
				else if (fOutside && sec[i] == '[')
					fOutside = false;
			}
            return sec.Length - 1;		// 就是专门解析棋谱文件的字符串操作...我们没必要花时间在这个上面
		}

        static private string PurgeCRLFSuffix(string inStr)
        {
            int iLast = inStr.Length - 1; // 就像词法分析跳过没有意义的字符一样

            if (iLast <= 0)
                return inStr; 

            while ((inStr[iLast] == '\r' || inStr[iLast] == '\n' || inStr[iLast] == ' '))
            {
                iLast--; 
            }
            if ((iLast+1) != inStr.Length)
                return inStr.Substring(0, iLast + 1);  //就像词法分析跳过没有意义的字符一样
            else
                return inStr; 
        }


        // 棋谱主体部分
		Boolean ParseASection(String sec) 
		{
			int iKey = 0;
			int iValue = 0;
			int iLastValue = 0;
			KeyValuePair kv;
			ArrayList Section = new ArrayList(10);
			
			try 
			{
				iKey = sec.IndexOf("[");
				if (iKey < 0)
				{
					return false;
				}
                sec = PurgeCRLFSuffix(sec);

                iValue = FindEndofValueStr(sec); // 提取一个[]操作
				iLastValue = sec.LastIndexOf("]");
				if (iValue <= 0 || iLastValue <= 1)
				{
					return false;
				}
				sec = sec.Substring(0,iLastValue+1);
                while (iKey > 0 && iValue > iKey)// 正确提取了一个[]
				{
					string key = sec.Substring(0,iKey);
					int iNonLetter = 0;
					while (!char.IsLetter(key[iNonLetter]) && iNonLetter < key.Length)
						iNonLetter ++;
					key = key.Substring(iNonLetter);
                    // X[]
					string strValue = sec.Substring(iKey+1, iValue-iKey-1);
                    // 键值对
					kv = new KeyValuePair(key, strValue);
					Section.Add(kv);
					if (iValue >= sec.Length)
						break;
					sec = sec.Substring(iValue+1);
					iKey = sec.IndexOf("[");
					if (iKey > 0)
					{
                        iValue = FindEndofValueStr(sec); // 循环继续
					}
				}
			}
			catch
			{
                return false;
            }

			ProcessASection(Section);
			return true;
		}


        // 提取出操作后就要进行识别了
        Boolean ProcessASection(ArrayList arrKV) 
		{
            Boolean fMultipleMoves = false;   //单步操作
			GoMove gm = null; 
            
			string key, strValue;

			for (int i = 0;i<arrKV.Count;i++)
			{
				key = ((KeyValuePair)(arrKV[i])).k;
				for (int j=0; j<((KeyValuePair)(arrKV[i])).alV.Count; j++)
				{
					strValue = (string)(((KeyValuePair)(arrKV[i])).alV[j]);

                    if (key.Equals("B"))   // 黑
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.Black);
                    }
                    else if (key.Equals("W"))  // 白
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.White);
                    }
                    else if (key.Equals("C")) // Comment,针对一些步数发表自己的看法。。
                    {
                        // 初始comment
                        if (gm != null)
                            gm.Comment = strValue;
                        else	// appent comment
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("L"))  // 放子,就是一开始就有一些地方放了子
                    {
                        if (gm != null)
                            gm.AddLabel(strValue);
                        else	// 中途放子是个什么逻辑
                            _stGameComment += strValue;
                    }

                    else if (key.Equals("M")) // 貌似是在开始之前就显示一些重要的操作？
                    {
                        if (gm != null)
                            gm.AddMark(strValue);
                        else	// 游戏中途搞这个？
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("AW"))		// 突然觉得好蛋疼，给这么一串英文标识符让我们来猜意思？
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.White);
                    }
                    else if (key.Equals("AB"))		// 多步黑
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.Black);
                    }
                    else if (key.Equals("HA"))//这些键值对根本就没有被用过..
                        _gi.handicap = (strValue);
                    else if (key.Equals("BR"))
                        _gi.rankBlack = (strValue);
                    else if (key.Equals("PB"))
                        _gi.playerBlack = (strValue);
                    else if (key.Equals("PW"))
                        _gi.playerWhite = (strValue);
                    else if (key.Equals("WR"))
                        _gi.rankWhite = (strValue);
                    else if (key.Equals("DT"))
                        _gi.date = (strValue);
                    else if (key.Equals("KM"))
                        _gi.km = (strValue);
                    else if (key.Equals("RE"))
                        _gi.result = (strValue);
                    else if (key.Equals("SZ"))
                        _gi.size = (strValue);
                    else if (key.Equals("EV"))
                        _gi.gameEvent = (strValue);
                    else if (key.Equals("PC"))
                        _gi.location = (strValue);
                    else if (key.Equals("TM"))
                        _gi.time = (strValue);
                    else if (key.Equals("GN"))
                        _gi.gameName = strValue;

                    else if (key.Equals("FF"))
                        _gi.unknown_ff = (strValue);
                    else if (key.Equals("GM"))
                        _gi.unknown_gm = (strValue);
                    else if (key.Equals("VW"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("US"))
                        _gi.unknown_vw = (strValue);

                    else if (key.Equals("BS"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("WS"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("ID"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("KI"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("SO"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("TR"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("LB"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("RO"))
                        _gi.unknown_vw = (strValue);


                    // 未定义的键值对
                    else
                        System.Diagnostics.Debug.Assert(false, "unhandle key: " + key + " "+ strValue);

                    // 如果是多步操作
                    if (fMultipleMoves)
                    {
                        _currVar.AddAMove(gm);
                    }
                }
            }

            // 下一步
            if (!fMultipleMoves && gm != null)
            {
                _currVar.AddAMove(gm);
            }
			return true;
		} 

		public GoMove DoPrev() 
		{
			return _currVar.DoPrev();
		}

		public GoMove PeekPrev() 
		{
			return _currVar.PeekPrev();
		}

		public GoMove DoNext() 
		{
			return _currVar.DoNext();
		}

		public void UpdateResult(GoMove gm) 
		{
			_currVar.UpdateResult(gm);
		}
		
		public void Reset()
		{
//			_currVarNum = 0;
			_currVarId = 0; 
			_currVar.Reset();
		}
		static public void RewindToStart()
		{

		}
	} 
}
