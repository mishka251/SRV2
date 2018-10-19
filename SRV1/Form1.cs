using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace SRV1
{
    public partial class Form1 : Form
    {

        Mutex mutex;
        Queue<int> queue;
        bool work = true;

        Semaphore semaphore;

        public Form1()
        {
            InitializeComponent();
            r = new Random();
            t1 = new Thread(new ThreadStart(tr1));

            mutex = new Mutex();
            queue = new Queue<int>();

            ThreadPool.GetMaxThreads(out nWorkerThreads, out nCompletionThreads);
            label2.Text += " в " + nWorkerThreads + " потоков";
            semaphore = new Semaphore(0, nWorkerThreads);
        }
        int nWorkerThreads, nCompletionThreads;

        Random r;
        int GetNum()
        {
            return r.Next(1, 9);
        }

        void tr1()
        {
            while (work)
            {
                int num = GetNum();

                mutex.WaitOne();
                queue.Enqueue(num);
                int[] arr = new int[queue.Count];

                lbBuf.Invoke(new Action(() => lbBuf.Items.Add(num.ToString())));
                try
                {
                    semaphore.Release();
                }
                catch
                {

                }
                mutex.ReleaseMutex();


                Thread.Sleep(r.Next(10, 20));
            }
        }

        void tr2(object state)
        {
            while (work)
            {
                semaphore.WaitOne();
                mutex.WaitOne();
                bool has_elem = queue.Count > 0;
                int num = 0;
                if (has_elem)
                {
                    num = queue.Dequeue();
                    lbBuf.Invoke(new Action(() => lbBuf.Items.RemoveAt(0)));
                }
                mutex.ReleaseMutex();
                string str = "Поток " + Thread.CurrentThread.ManagedThreadId + " обрабатывает " + num;
                lbWork.Invoke(new Action(() => lbWork.Items.Add(str)));

                
                Thread.Sleep(r.Next(1500, 250000));
                lbWork.Invoke(new Action(() => lbWork.Items.Remove(str)));
               
            }
        }


        Thread t1;



        private void btnStart_Click(object sender, EventArgs e)
        {
            t1.Start();
            
            for (int i = 0; i < nWorkerThreads; i++)
                ThreadPool.QueueUserWorkItem(tr2);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            work = false;
            Thread.Sleep(300);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            work = false;
            Thread.Sleep(250);
            Application.Exit();
        }
    }
}
