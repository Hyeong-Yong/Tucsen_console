using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using TUCAMAPI;

namespace C.NET01_get_info
{
    class Program
    {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object
        public static TUCAM_VALUE_INFO m_viCam; // Value info object
        public static TUCAM_REG_RW m_regRW;     // Register read/write

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

        /* Print the camera information */
        static void PrintCameraInfo()
        {
            string strVal;
            string strText;
            IntPtr pText = Marshal.AllocHGlobal(64);

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_CAMERA_MODEL;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam))
            {
                strText = Marshal.PtrToStringAnsi(m_viCam.pText);

                Console.WriteLine("Camera  Name    : {0}", strText);
            }

            m_regRW.nRegType = (int)TUREG_TYPE.TUREG_SN;
            m_regRW.nBufSize = 64;
            m_regRW.pBuf = pText;

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Reg_Read(m_opCam.hIdxTUCam, m_regRW))
            {
                strText = Marshal.PtrToStringAnsi(m_regRW.pBuf);
                Console.WriteLine("Camera  SN      : {0}", strText);
            }
            
            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VENDOR;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam))
            {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Camera  VID     : 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_PRODUCT;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam))
            {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Camera  PID     : 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_CAMERA_CHANNELS;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam))
            {
                Console.WriteLine("Camera  Channels: {0}", m_viCam.nValue);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_BUS;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam))
            {
                if (0x200 == m_viCam.nValue || 0x210 == m_viCam.nValue)
                {
                    Console.WriteLine("USB     Type    : {0}", "2.0");
                }
                else
                {
                    Console.WriteLine("USB     Type    : {0}", "3.0");
                }
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VERSION_FRMW;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam))
            {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Version Firmware: 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VERSION_API;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_viCam))
            {
                strText = Marshal.PtrToStringAnsi(m_viCam.pText);
                Console.WriteLine("Version API     : {0}", strText);
            }

            Marshal.Release(pText);    
        }

        static void Main(string[] args)
        {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
            {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0))
                {
                    Console.WriteLine("Open the camera success");
                    Console.WriteLine();
                    PrintCameraInfo();
                    Console.WriteLine();
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
    }
}
