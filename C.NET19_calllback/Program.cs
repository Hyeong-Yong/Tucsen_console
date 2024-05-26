using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUCAMAPI;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace C.NET19_calllback
{
    class Program
    {
        public static TUCAM_INIT m_itApi;          // SDK API initialized object
        public static TUCAM_OPEN m_opCam;          // Open camera object
        public static TUCAM_FRAME m_frame;
        public static TUCAM_FILE_SAVE m_fs;        //file save

        public static TUCamAPI.BUFFER_CALLBACK m_BufCallBack;
        public static CallBack m_callback;

        public static int m_nCount = 20;

        static void Main(string[] args)
        {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
            {
                m_opCam.uiIdxOpen = 0;
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0))
                {
                    Console.WriteLine("Open Camera success");

                    m_callback = new CallBack();

                    m_BufCallBack = m_callback.BuffCallBack;
                    //m_BufCallBack = BuffCallBack;

                    //注册回调函数
                    TUCAMRET regisRet = TUCamAPI.TUCAM_Buf_DataCallBack(m_opCam.hIdxTUCam, m_BufCallBack, (IntPtr)GCHandle.Alloc(m_callback));
                    Console.WriteLine("set callback,code={0:x}", regisRet);

                    CapStart();

                    Thread.Sleep(TimeSpan.FromSeconds(5));

                    Console.WriteLine("0000000");

                    TUCamAPI.TUCAM_Buf_AbortWait(Program.m_opCam.hIdxTUCam);
                    Console.WriteLine("1111111");

                    TUCamAPI.TUCAM_Cap_Stop(Program.m_opCam.hIdxTUCam);
                    Console.WriteLine("2222222");

                    TUCamAPI.TUCAM_Buf_Release(Program.m_opCam.hIdxTUCam);
                    Console.WriteLine("333333");

                    //TUCamAPI.TUCAM_Cap_ClearBuffer(Program.m_opCam.hIdxTUCam);
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Open Camera fail!");
                }
            }
        }

        /* Init the TUCAM API */
        static TUCAMRET InitApi()
        {
            /* Get the current directory */
            IntPtr strPath = Marshal.StringToHGlobalAnsi(System.Environment.CurrentDirectory);

            m_itApi.uiCamCount = 0;
            m_itApi.pstrConfigPath = strPath;

            TUCamAPI.TUCAM_Api_Init(ref m_itApi);

            Console.WriteLine("Connect {0} camera", m_itApi.uiCamCount);

            if (0 == m_itApi.uiCamCount)
            {
                return TUCAMRET.TUCAMRET_NO_CAMERA;
            }

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        /* UnInit the TUCAM API */
        static TUCAMRET UnInitApi()
        {
            return TUCamAPI.TUCAM_Api_Uninit();
        }

        /* Open the camera by index number */
        static TUCAMRET OpenCamera(uint uiIdx)
        {
            if (uiIdx >= m_itApi.uiCamCount)
            {
                return TUCAMRET.TUCAMRET_OUT_OF_RANGE;
            }

            m_opCam.uiIdxOpen = uiIdx;

            return TUCamAPI.TUCAM_Dev_Open(ref m_opCam);
        }

        /* Close the current camera */
        static TUCAMRET CloseCamera()
        {
            if (null != m_opCam.hIdxTUCam)
            {
                TUCamAPI.TUCAM_Dev_Close(m_opCam.hIdxTUCam);
            }

            Console.WriteLine("Close the camera success");

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        //wait for frame
        static TUCAMRET CapStart()
        {
            /* Get the current directory */
            //IntPtr strPath = Marshal.StringToHGlobalAnsi(System.Environment.CurrentDirectory);

            IntPtr pBuf;
            //int nSize = 1024 * 1024 * 100; //10M
            IntPtr[] pBufArr = new IntPtr[m_nCount];
            List<IntPtr> bufList = new List<IntPtr>();

            m_frame.pBuffer = IntPtr.Zero;
            m_frame.uiRsdSize = 1;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;

            //alloc tuframe buffer
            TUCAMRET allocRet = TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);
            if (TUCAMRET.TUCAMRET_SUCCESS == allocRet)
            {
                Console.WriteLine("alloc buffer success,imgsize {0})", (uint)m_frame.uiImgSize);
            }
            else
            {
                Console.WriteLine("alloc buffer failure,err code :{0:X})", allocRet);
            }

            //Announce Buffer
            for (int i = 0; i < m_nCount; i++) {
                pBuf = Marshal.AllocHGlobal((int)m_frame.usWidth * m_frame.usHeight);

                TUCAMRET announceBufRet = TUCamAPI.TUCAM_Cap_AnnounceBuffer(m_opCam.hIdxTUCam, (uint)m_frame.usWidth * m_frame.usHeight, pBuf);
                if (TUCAMRET.TUCAMRET_SUCCESS == announceBufRet) {
                    bufList.Add(pBuf);
                }
                else {//
                    Console.WriteLine("camera announce buffer failure!,index ={0},error code={1:x}", i, announceBufRet);
                }
            }
            Console.WriteLine("camera announce succ!");


            //capture start
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_SEQUENCE))
            {
                Console.ReadKey();
            }

            return TUCAMRET.TUCAMRET_SUCCESS;
        }
    }

    class CallBack
    {

        public static TUCAM_RAWIMG_HEADER m_cbHeader;
        public static int m_cnt = 10;
        public IntPtr m_pBuf = IntPtr.Zero;


        public void BuffCallBack(IntPtr pUserContext)
        {
            Console.WriteLine("m_cnt == {0}", m_cnt);


            if (m_pBuf == IntPtr.Zero)
            {
                m_pBuf = Marshal.AllocHGlobal((int)Program.m_frame.uiImgSize);

                m_cbHeader.pImgData = m_pBuf;
            }

            TUCAMRET  getBufRet = TUCamAPI.TUCAM_Buf_GetData(Program.m_opCam.hIdxTUCam, ref m_cbHeader);

            if (TUCAMRET.TUCAMRET_SUCCESS == getBufRet)
            {
                save((int)m_cbHeader.uiImgSize);
                //Console.WriteLine("image width:{0},height:{1},offset:{2}", m_cbHeader.uiWidth, m_cbHeader.uiHeight,m_cbHeader.uiImgOffset);
            }

        }


        public static void save(int length)//存储
        {

            if (m_cnt > 0)
            {
                String imgDir = ".\\img";
                if (!Directory.Exists(imgDir))
                {
                    Directory.CreateDirectory(imgDir);
                }
                String strPath = string.Format(imgDir + "\\image_{0}.raw", m_cnt);

                byte[] bytes = new byte[length]; 
                Marshal.Copy(m_cbHeader.pImgData, bytes, 0, length);

                FileStream file = new FileStream(strPath, FileMode.Create, FileAccess.Write);
                file.Write(bytes, 0, length);
                file.Close();

                Console.WriteLine("save img successful!image path:{0}", strPath);
                m_cnt--;
            }

        }
    }
}
