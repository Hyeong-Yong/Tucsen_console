using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TUCAMAPI;

namespace C.NET17_timestamp
{
    class Program
    {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object
        public static TUCAM_FRAME m_frame;      // The frame object
        public static TUCAM_TRIGGER_ATTR m_tgr; // The trigger object
        public static CallBack m_callback;
        public static TUCamAPI.BUFFER_CALLBACK m_BufCallBack;
        public static TUCAM_IMG_HEADER m_imgHeader;


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

        static void Main(string[] args)
        {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
            {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0))
                {
                    Console.WriteLine("Open the camera success");

                    m_callback = new CallBack();
                    m_BufCallBack = m_callback.BuffCallBack;

                    //注册回调函数
                    TUCAMRET regisRet = TUCamAPI.TUCAM_Buf_DataCallBack(m_opCam.hIdxTUCam, m_BufCallBack, (IntPtr)GCHandle.Alloc(m_callback));

                    WaitForImageData();

                    CloseCamera();
                }
                else
                {
                    Console.WriteLine("Open the camera failure");
                }

                UnInitApi();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /* Wait for image data */
        static void WaitForImageData()
        {
            int nTimes = 3;

            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_SEQUENCE))
            {
                for (int i = 0; i < nTimes; ++i)
                {
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(m_opCam.hIdxTUCam, ref m_frame))
                    {
                        byte[] buf = new byte[m_frame.usHeader];
                        GCHandle handle = GCHandle.Alloc(m_imgHeader);
                        Marshal.Copy(m_frame.pBuffer, buf, 0, m_frame.usHeader);

                       //Console.WriteLine("Grab the frame success index number is {0}, time start:{1}, time last:{2}", m_frame.uiIndex, m_imgHeader.dblTimeStamp, m_imgHeader.dblTimeLast);
                    }
                    else
                    {
                        Console.WriteLine("Grab the frame failure, index number is {0}", i);
                    }
                }

                TUCamAPI.TUCAM_Buf_AbortWait(m_opCam.hIdxTUCam);
                TUCamAPI.TUCAM_Cap_Stop(m_opCam.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_Release(m_opCam.hIdxTUCam);
        }
    }

    class CallBack
    {
        public static TUCAM_RAWIMG_HEADER m_cbHeader;
        public static int m_cnt = 10;
        public IntPtr m_pBuf = IntPtr.Zero;


        public void BuffCallBack(IntPtr pUserContext)
        {
            //Console.WriteLine("TUCAM_Buf_GetData");

            if (m_pBuf == IntPtr.Zero)
            {
                m_pBuf = Marshal.AllocHGlobal((int)Program.m_frame.uiImgSize);

                m_cbHeader.pImgData = m_pBuf;
            }

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_GetData(Program.m_opCam.hIdxTUCam, ref m_cbHeader))
            {
                Console.WriteLine("TUCAM_Buf_GetData index number is {0}, time start:{1}, time last:{2}", m_cbHeader.uiIndex, m_cbHeader.dblTimeStamp, m_cbHeader.dblTimeLast);
            }

        }
    }
}
