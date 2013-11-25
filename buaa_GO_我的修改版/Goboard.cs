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

	public enum StoneColor : int //���Ӱ���
	{
		Black = 0, White = 1
	}


	/**
	 * �ǺǺ�
	 */
	public class GoBoard : System.Windows.Forms.Form
	{
		string [] strLabels; // {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"};

		int nSize;		                //ÿ��ÿ�з����� 19
		const int nBoardMargin = 10;	//���߾��� 10
		int nCoodStart = 4;
		const int	nBoardOffset = 20; //��������߾��� 20
		int nEdgeLen = nBoardOffset + nBoardMargin; //�������½ǵĺ��ݳ���
		int nTotalGridWidth = 360 + 36;	//�����ܴ�С
		int nUnitGridWidth = 22;		//ÿ��С����Ĵ�С
		int nSeq = 0;
		Rectangle rGrid;		    // rectangle for ��������
		StoneColor m_colorToPlay;   // ������Ҫ�ߵ�������ɫ
		GoMove m_gmLastMove;	    // ��һ������
		Boolean bDrawMark;	        // �Ƿ�highlight������ʾ(��˵�������ĺô�..)
		Boolean m_fAnyKill;	        // �Ƿ��г��Ӷ���
		Spot [,] Grid;		        // ��¼����״̬�Ķ�ά����
		Pen penGrid; //����ɫ��������ͼ
        //��ɾ����, penStoneW, penStoneB,penMarkW, penMarkB
		Brush brStar, brBoard, brBlack, brWhite, m_brMark; //���ֻ�ˢ������ͼ
	
        // >>Button <<Button
        int nFFMove = 10;   // ����>>Button����������
  //      int nRewindMove = 10;  // ����<<Button����������,��1.�������û��ʹ�ù� 2.<<Button click��ʱ��ֱ�Ӿ�reset��,���Ը���û��

		GoTree	gameTree; //��Ϸ�߼�����

		///    ����UI��������
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
			// Form��һ��,��ʼ���������
			//
			InitializeComponent();

			//
			// ���ֳ�ʼ������
			//

			this.nSize = nSize;  //�趨���̴�С

			m_colorToPlay = StoneColor.Black; //ִ������

			Grid = new Spot[nSize,nSize]; //newһ��Grid��״̬
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					Grid[i,j] = new Spot();
            /*------���¸��ֳ�ʼ�����ʻ�ˢ------*/
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
        /*------����UI------*/
        #region
		///    ��̬���Ƹ������,ûʲô��˵��
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
			UpdateGoBoard(e); //��������
		}
        /*------Save���ܶ�û��ʵ��?------*/
        protected void SaveClick(object sender, System.EventArgs e)
        {
            SaveFile();
            return;
        }
        /*------��˵��Щ�򵥵�button��Ӧ�¼��Ͳ��ö�˵�˰�?------*/
        protected void OpenClick(object sender, System.EventArgs e)
        {
            OpenFile();
            ShowGameInfo();//�����ļ����ܴ���������Ϣ
        }
        #region
        protected void RewindClick(object sender, System.EventArgs e)
        {
            gameTree.Reset();//<<Button��Ҫ�����Ϸ�߼�����,��������,��������ʾ��Ϸ��Ϣ
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
                    if (i++ > nFFMove)//������״̬�ָ�������ָ���������״̬
                        break;
                }
            }
        }

        protected void ForwardClick(object sender, System.EventArgs e)
        {
            GoMove gm = gameTree.DoNext();
            if (null != gm)//ǰ��������ʷ��¼����һ������
            {
                PlayNext(ref gm);
            }
        }
		private void ShowGameInfo()
		{
			//��ʾ��Ϸ��Ϣ
			textBox1.Clear();
			textBox1.AppendText(gameTree.Info);
		}

        protected void BackClick(object sender, System.EventArgs e)
        {
            GoMove gm = gameTree.DoPrev();	//��Ϸ��ʷ��¼�е�ǰһ��
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

		Boolean OnBoard(int x, int y) //�߽紦��
		{
			return (x>=0 && x<nSize && y>=0 && y<nSize);
		}
        /*------����û�ÿ�ɾ�ķ�����------*/
        protected void GoBoardClick(object sender, System.EventArgs e)
        {
            return;
        }
        /*------������ת��Ϊ�����е�ͼ��------*/
		private Point PointToGrid(int x, int y)
		{
			Point p= new Point(0,0);
			p.X = (x - rGrid.X + nUnitGridWidth/2) / nUnitGridWidth;
			p.Y = (y - rGrid.Y + nUnitGridWidth/2) / nUnitGridWidth;
			return p;
		}

		//�趨�����ཻ�㸽�������ķ�Χ���ɿ�������Ϊ�ڴ˴�����
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
        /// ����ɿ��¼�,������������
		private void MouseUpHandler(Object sender,MouseEventArgs e)
		{
			Point p;
			GoMove	gmThisMove;

			p = PointToGrid(e.X,e.Y);
			if (!OnBoard(p.X, p.Y) || !CloseEnough(p,e.X, e.Y)|| Grid[p.X,p.Y].HasStone())
				return; //��������������

			//������������ʱ,����,����thismove����gametree��
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
			Point p = gm.Point; //��õ�ǰmove��
			m_colorToPlay = gm.Color;	//������Ҫ���ӵ���ɫ

			//���highlight��Ϣ��������Ϣ
			ClearLabelsAndMarksOnBoard();

            if (m_gmLastMove != null)//�����һ���Ѿ�����,��ôȡ�������ӵĸ�����ʾ
            {
                RepaintOneSpotNow(m_gmLastMove.Point);
            }
			bDrawMark = true; //������ʾ
			Grid[p.X,p.Y].SetStone(gm.Color); //����
			m_gmLastMove = new GoMove(p.X, p.Y, gm.Color, nSeq++); //�����β�����¼��lastmove��
			//������ʾ���е�labels��mark
			SetLabelsOnBoard(gm);
			SetMarksOnBoard(gm);
			
			DoDeadGroup(NextTurn(m_colorToPlay));//nextturn������һ�ֵ�������ɫ����һ���������г��Ӷ���,ע�����ж����������ܷ��ȳԶԷ�
			//����г��ӵĶ�������ô�ʹ���deadgroup�� 
            if (m_fAnyKill)
            {
                AppendDeadGroup(ref gm, NextTurn(m_colorToPlay));
            }
            else //����Ҫ���Ƿ�ᱻ��
            {
                DoDeadGroup(m_colorToPlay);
                if (m_fAnyKill)
                {
                    AppendDeadGroup(ref gm, m_colorToPlay);
                }
            }
			m_fAnyKill = false;
			
			OptRepaint();//�ػ�����

			//������һ��������ɫ
			m_colorToPlay = NextTurn(m_colorToPlay);
			
			//......������Ϸ��Ϣ,������ʵ������дshowgameinfo()
			textBox1.Clear();
			textBox1.AppendText(gm.Comment);
		}
        /*------��ӱ��Ե���ɫΪc����------*/
		private void AppendDeadGroup(ref GoMove gm, StoneColor c)
		{
			ArrayList a = new ArrayList();
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].IsKilled())
					{
						Point pt = new Point(i,j);
						a.Add(pt);
						Grid[i,j].SetNoKilled();//����...����б�Ҫ��װ���������ô..
					}
			gm.DeadGroup = a;//���뱾�ζ���gm��,gm.deadgroup�ʹ���˱��ֱ��Ե���,����playprev�����õ�
			gm.DeadGroupColor = c;
		}
        /*------�ػ�����------*/
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
            Point p = gm.Point;//��õ�ǰҪ�Ƴ��ĵ�
            m_colorToPlay = gm.Color;//Ҫ�Ƴ��ĵ����ɫ
            ClearLabelsAndMarksOnBoard();//���highlight��Ϣ��������Ϣ 
            Grid[p.X, p.Y].RemoveStone();//remove the current move from the board
            bDrawMark = false;//also remove the "lastmove" highlight
            RepaintOneSpotNow(p); 
            if (gm.DeadGroup != null)//�ָ����Ե�����
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

				
		
		Rectangle GetUpdatedArea(int i, int j) //������Ҫ�����ػ������
		{
			int x = rGrid.X + i * nUnitGridWidth - nUnitGridWidth/2;
			int y = rGrid.Y + j * nUnitGridWidth - nUnitGridWidth/2;
			return new Rectangle(x,y, nUnitGridWidth, nUnitGridWidth);
		}

		/**
		 * �ػ�
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
		 * ֻ�ػ�һ�������,���ڱ����Ѿ�������Ҫ����>>����<<������ʱ��
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

		//������˼�Ǽ�¼�����������������û���õ������ܿ��� 
        public void RecordMove(Point point, StoneColor colorToPlay) 
		{
            Grid[point.X, point.Y].SetStone(colorToPlay);
			// ����һ��������Ϊ�ôβ���
            m_gmLastMove = new GoMove(point.X, point.Y, colorToPlay, nSeq++);
		}
        //�����´����ӵ���ɫ
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
         *	˵���˾���dfs������������Ϊ0����
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
        /*------���ɨ���¼,�������visit�������˼------*/
		void CleanScanStatus()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].ClearScanned();
		}

		/**
		 * ɨ����������,�Ե�ǰ��ɫc,���������Ϊ0����
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
        /*------�ǵݹ�BFSʵ�ּ�����------*/
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
		 * dfs����ÿ����(ÿ��group)����
		 */
		int CalcLiberty(int x, int y, StoneColor c) 
		{
			int lib = 0; // ��ʼ��
			
			if (!OnBoard(x,y))
				return 0;
			if (Grid[x,y].IsScanned())
				return 0;

			if (Grid[x,y].HasStone()) 
			{
				if (Grid[x,y].Color() == c) 
				{
					//dfs�����ĸ����ڵĸ���
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
			{// ��Χû�����ӵĻ���+1
				lib ++;
				Grid[x,y].SetScanned();
			}

			return lib;
		}


		/**
		 * ������ʾ��һ�ε�����
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
        /*------�����������Ӻ͸������------*/
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
        /*------���ǰ��յ�ǰ����gm,��gm֮ǰ��labelsȫ������------*/
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
        /*------���������ϵĸ�����ʾ------*/
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
        
		static private Point SwapXY(Point p)//��...������������
		{
			Point pNew = new Point(p.Y,p.X);
			return pNew;
		}
        /*------��������------*/
        private void DrawBoard(Graphics g)
		{
			//��������
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
			//���Ʊ���
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
			
			//������Ԫ
			DrawStars(e.Graphics);

			//����ÿһ������
			DrawEverySpot(e.Graphics);
		}

		//������Ԫ
		void DrawStar(Graphics g, int row, int col) 
		{
			g.FillRectangle(brStar,
				rGrid.X + (row-1) * nUnitGridWidth - 1, 
				rGrid.Y + (col-1) * nUnitGridWidth - 1, 
				3, 
				3);
		}

		//����9����Ԫ
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
		 * ��������
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
        /*------�������ò��û�е��ù�?------*/
		void DrawLabel(Graphics g, int x, int y, short nLabel) 
		{
			if (nLabel ==0)
				return;
			nLabel --;
			nLabel %= 18;			//����ת��

			//��Label
			Rectangle r = new Rectangle(rGrid.X+ x * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(brBoard, r);
			g.DrawString(strLabels[nLabel],	//���ַ���
				this.Font, 
				brBlack, 
				rGrid.X+ (x) * nUnitGridWidth - (nUnitGridWidth-1)/4, 
				rGrid.Y + (y) * nUnitGridWidth - (nUnitGridWidth-1)/2);
		}
        /*------����highlight��------*/
		void DrawMark(Graphics g, int x, int y)
		{
			g.FillRectangle( m_brMark,
				rGrid.X + x* nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/8,
				5, 5);
		}
        /*------����ÿһ�����ӵ�------*/
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
			//��ǲ���
			if (bDrawMark && m_gmLastMove != null)
				MarkLastMove(g);
		}
        #endregion
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:��Ҫ����ͷŶ���"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:�벻Ҫ���ı���Ϊ���ػ���������", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:�벻Ҫ���ı���Ϊ���ػ���������", MessageId = "System.Windows.Forms.FileDialog.set_Filter(System.String)")]
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
                            string s = gameTree.Info;//����Ӧ�õݹ��gm����Ϣ,����gmĿǰ����ȫ�ֱ���.
                            w.WriteLine(s);
                        }
                    }
                }
            }
        }
        //�������ļ�
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
		/// main���,���̱߳��
		/// 
        [STAThread]
		public static void Main(string[] args) 
		{
			Application.Run(new GoBoard(19));
		}
	}

	
	//Spot�����ÿһ����
	public class Spot 
	{
		private Boolean bEmpty; //�Ƿ�Ϊ��
		private Boolean bKilled; //�Ƿ񱻳�
		private Stone s; //��������
		private short	m_nLabel; 
		private Boolean m_bMark; //�Ƿ����
		private Boolean bScanned; //�Ƿ�ɨ���
		private Boolean bUpdated; //�Ƿ��Ѿ�������
		/**
		 * ���캯��
		 */
		public Spot() 
		{
			bEmpty = true;
			bScanned = false;
			bUpdated = false;
			bKilled = false;
		}
		/*------�򵥸��ַ���------*/
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
			} // ����
		}

		/*
		 * ����
		*/
		public void RemoveStone()
		{	//����
			bEmpty = true;
			bUpdated = true;
		}
				
		//����
		public void Die() 
		{
			bKilled = true;
			bEmpty = true;
			bUpdated = true;
		} 

		public Boolean IsKilled() { return bKilled;}
		public void SetNoKilled() { bKilled = false;}

		public void ResetUpdated() { bUpdated = false; bKilled = false;}

		//�Ƿ񱻸���
		public Boolean IsUpdated() 
		{ 
			if (bUpdated)
			{	//����Ѿ���������,��ô�ø���״̬Ϊ��
				bUpdated = false;
				return true;
			} 
			else 
				return false;
		}

		// updated��set����
		public void SetUpdated() { bUpdated = true; }
	}

	/**
	 * ������
	 */
	public class GoMove 
	{
		StoneColor m_c;	//������ɫ
		Point m_pos;		//����λ��
		int m_n;			//������
		String m_comment;	// ÿ����������Ϣ
		MoveResult m_mr;	//ÿ�������Ľ��

		ArrayList		m_alLabel; //���е�label 
		ArrayList		m_alMark; //���е�mark

		//���еı��Ե�
		//�����ӵ���ɫ 
		ArrayList		m_alDead;
		StoneColor	m_cDead;
		/**
		 * ���캯��
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
        /*------���ֹ��캯�����������ļ�------*/
		public GoMove(String str, StoneColor color) 
		{
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
			char cx = str[0];
			char cy = str[1];
			m_pos = new Point(0,0);
			//ת��Ϊ����
			m_pos.X = (int) ( (int)cx - (int)(char)'a');
			m_pos.Y = (int) ( (int)cy - (int)(char)'a');
			this.m_c = color;
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}

        /*------���ǽ���Ϸ��Ϣ�е�strת��Ϊ������,����˵����Ĺ��캯�����븲����̫��------*/
		static private Point	StrToPoint(String str)
		{
			Point p = new Point(0,0);
			char cx = str[0];
			char cy = str[1];
			//ת��Ϊ����
			p.X = (int) ( (int)cx - (int)(char)'a');
			p.Y = (int) ( (int)cy - (int)(char)'a');
			return p;
		}
        /*------����get/set------*/
        //������ɫ
        public StoneColor Color
        { 
            get { return m_c; } 
        }
        //��Ϸ��Ϣ
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
	 * ���������
	 * 
	 */
	public class MoveResult 
	{
        public StoneColor color; 
		// 4�������Ƿ񱻳��� 
        public Boolean bUpKilled;
        public Boolean bDownKilled;
        public Boolean bLeftKilled;
		public Boolean bRightKilled;
		public Boolean bSuicide;	//�Ƿ�����
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
	 * ...�����ڸ�ʲô..ͻȻ�ֳ���һ��ֻ��һ����Ա��struct..
	 */
	public struct Stone 
	{
		public StoneColor color; 
	}

	/**
	 * ����������
	 * 
	 */
	public class GoVariation 
	{
	//	int m_id;			//�ò���ɾ����
	//	string m_name;	//�ò���ɾ����
		//��ֱ�������� 	//��֪������˵ʲô	
		ArrayList m_moves; 
		int m_seq;			//�������Ӧ�����ڶ��׵�ʱ������õ� 
		int m_total;

		//���캯��
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
		 *  ���������ȷʵ�����ã������ز������еĵ�ǰ������ǰһ��
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
	* ����˵�������ʵ�ò��� 
	* ������ò����� 
	*/
    //struct VarStartPoint
    //{
    //    int m_id; 
    //    int m_seq;
    //}

	struct GameInfo //���GameInfo��ʵ�кö��ò����ĵط�
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
        public string time;             // ʱ��
        public string unknown_ff;   //˭�ܸ���������ʲô...
        public string unknown_gm;
        public string unknown_vw; 
	}

    /*------��һ��Ӧ���ǽ���������------*/

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
				if (strOut[iSlash] == '\\')		// && ת���ַ����ȣ����ַ�\
				{
					strOut = strOut.Remove(iSlash,1);
					if (iSlash>0)
						iSlash --;	// ����ר�Ž��������ļ����ַ�������...����û��Ҫ��ʱ�����������
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

            // ��������Ϣת��ΪX[]����ʽ
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
            if (iBegin == -1)	// ����������
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
                    strOneVal = v.Substring(iBegin);	// ����������
				alV.Add(strOneVal);
				iBegin = v.IndexOf("[", iBegin+1);
				if (iBegin > 0)
                    iBegin++;	// ѭ������
			}
		}
	}

	//GoTree��ʵ�ǲ�������

	public class GoTree 
	{
		GameInfo _gi;		//_gi:GameInfo��Ϸ��Ϣ
		ArrayList _vars;		//forward��ʱ����
		int _currVarId;		//��ǰ����ID
//		int _currVarNum;
		GoVariation _currVar;		//��ǰ����
		string	_stGameComment;
       
		// �����ײ����Ĺ��캯��
		public GoTree(string str)
		{
			_vars = new ArrayList(10);
//			_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
			ParseFile(str); // ������Ϣת��
		}

		//	ƽʱ�õĹ��캯��
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
		 * ����˼��,�����ļ�ת���õ�
		 */
		Boolean ParseFile(String goStr) 
		{
			int iBeg, iEnd=0; 
			while (iEnd < goStr.Length) 
			{
				if (iEnd > 0)
					iBeg = iEnd;
				else 
					iBeg = goStr.IndexOf(";", iEnd);//��iEnd���һ��;��λ��
				iEnd = goStr.IndexOf(";", iBeg+1);//indexof���δ�ҵ��᷵��-1
                if (iEnd < 0) // û�ҵ�
                    iEnd = goStr.LastIndexOf(")", goStr.Length);		//// �����һ����
				if (iBeg >= 0 && iEnd > iBeg) 
				{
					string section = goStr.Substring(iBeg+1, iEnd-iBeg-1);
					ParseASection(section);//���ֳ��������岿�ֿ�ʼ��
				} 
				else 
					break;
			}
			return true;
		}

        /// �൱�ڴʷ�������ȡһ������
        static public int FindEndofValueStr(String sec)
        {
            if (sec == null)
            {
                throw new ArgumentNullException("sec");
            }
            int i = 0;
            // �ص�]
            while (i >= 0)
            {
                i = sec.IndexOf(']', i+1);
                if (i > 0 && sec[i - 1] != '\\')
                    return i;    // ���ص���λ��
            }

            // û�ᵽ���ʾͷ����ܳ���
            return sec.Length - 1;		// û�ᵽ���ʾͷ����ܳ���
        }
        
        static public int FindEndofValueStrOld(String sec)//�ϰ�,�ò���
		{
            if (sec == null)
            {
                throw new ArgumentNullException("sec");
            }
			int i = 0;
            // ����ר�Ž��������ļ����ַ�������...����û��Ҫ��ʱ�����������
			bool fOutside = false;
			
			for (i=0; i<sec.Length;i++)
			{
				if (sec[i] == ']')
				{
                    if (i > 1 && sec[i - 1] != '\\') // ����ר�Ž��������ļ����ַ�������...����û��Ҫ��ʱ�����������
						fOutside = true;
				}
				else if (char.IsLetter(sec[i]) && fOutside && i>0)
					return i-1;
				else if (fOutside && sec[i] == '[')
					fOutside = false;
			}
            return sec.Length - 1;		// ����ר�Ž��������ļ����ַ�������...����û��Ҫ��ʱ�����������
		}

        static private string PurgeCRLFSuffix(string inStr)
        {
            int iLast = inStr.Length - 1; // ����ʷ���������û��������ַ�һ��

            if (iLast <= 0)
                return inStr; 

            while ((inStr[iLast] == '\r' || inStr[iLast] == '\n' || inStr[iLast] == ' '))
            {
                iLast--; 
            }
            if ((iLast+1) != inStr.Length)
                return inStr.Substring(0, iLast + 1);  //����ʷ���������û��������ַ�һ��
            else
                return inStr; 
        }


        // �������岿��
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

                iValue = FindEndofValueStr(sec); // ��ȡһ��[]����
				iLastValue = sec.LastIndexOf("]");
				if (iValue <= 0 || iLastValue <= 1)
				{
					return false;
				}
				sec = sec.Substring(0,iLastValue+1);
                while (iKey > 0 && iValue > iKey)// ��ȷ��ȡ��һ��[]
				{
					string key = sec.Substring(0,iKey);
					int iNonLetter = 0;
					while (!char.IsLetter(key[iNonLetter]) && iNonLetter < key.Length)
						iNonLetter ++;
					key = key.Substring(iNonLetter);
                    // X[]
					string strValue = sec.Substring(iKey+1, iValue-iKey-1);
                    // ��ֵ��
					kv = new KeyValuePair(key, strValue);
					Section.Add(kv);
					if (iValue >= sec.Length)
						break;
					sec = sec.Substring(iValue+1);
					iKey = sec.IndexOf("[");
					if (iKey > 0)
					{
                        iValue = FindEndofValueStr(sec); // ѭ������
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


        // ��ȡ���������Ҫ����ʶ����
        Boolean ProcessASection(ArrayList arrKV) 
		{
            Boolean fMultipleMoves = false;   //��������
			GoMove gm = null; 
            
			string key, strValue;

			for (int i = 0;i<arrKV.Count;i++)
			{
				key = ((KeyValuePair)(arrKV[i])).k;
				for (int j=0; j<((KeyValuePair)(arrKV[i])).alV.Count; j++)
				{
					strValue = (string)(((KeyValuePair)(arrKV[i])).alV[j]);

                    if (key.Equals("B"))   // ��
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.Black);
                    }
                    else if (key.Equals("W"))  // ��
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.White);
                    }
                    else if (key.Equals("C")) // Comment,���һЩ���������Լ��Ŀ�������
                    {
                        // ��ʼcomment
                        if (gm != null)
                            gm.Comment = strValue;
                        else	// appent comment
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("L"))  // ����,����һ��ʼ����һЩ�ط�������
                    {
                        if (gm != null)
                            gm.AddLabel(strValue);
                        else	// ��;�����Ǹ�ʲô�߼�
                            _stGameComment += strValue;
                    }

                    else if (key.Equals("M")) // ò�����ڿ�ʼ֮ǰ����ʾһЩ��Ҫ�Ĳ�����
                    {
                        if (gm != null)
                            gm.AddMark(strValue);
                        else	// ��Ϸ��;�������
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("AW"))		// ͻȻ���úõ��ۣ�����ôһ��Ӣ�ı�ʶ��������������˼��
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.White);
                    }
                    else if (key.Equals("AB"))		// �ಽ��
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.Black);
                    }
                    else if (key.Equals("HA"))//��Щ��ֵ�Ը�����û�б��ù�..
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


                    // δ����ļ�ֵ��
                    else
                        System.Diagnostics.Debug.Assert(false, "unhandle key: " + key + " "+ strValue);

                    // ����Ƕಽ����
                    if (fMultipleMoves)
                    {
                        _currVar.AddAMove(gm);
                    }
                }
            }

            // ��һ��
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
