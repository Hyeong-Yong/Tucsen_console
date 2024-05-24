using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using TUCAMAPI;

namespace C.NET02_get_infoex
{
    class Program
    {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_VALUE_INFO m_viCam; // Value info object

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

        /* Print the camera information by index number */
        static void PrintCameraInfoEx(uint uiIdx)
        {
            if (uiIdx >= m_itApi.uiCamCount)
            {
                Console.WriteLine("PrintCameraInfoEx: The index number of camera is out of range.");
                return;
            }

            string strVal;
            string strText;
            IntPtr pText = Marshal.AllocHGlobal(64);

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_CAMERA_MODEL;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam))
            {
                strText = Marshal.PtrToStringAnsi(m_viCam.pText);

                Console.WriteLine("Camera  Name    : {0}", strText);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VENDOR;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam))
            {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Camera  VID     : 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_PRODUCT;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam))
            {
                strVal = String.Format("{0:X000}", m_viCam.nValue);
                Console.WriteLine("Camera  PID     : 0x{0}", strVal);
            }

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_BUS;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam))
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

            m_viCam.nID = (int)TUCAM_IDINFO.TUIDI_VERSION_API;
            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Dev_GetInfoEx(uiIdx, ref m_viCam))
            {
                strText = Marshal.PtrToStringAnsi(m_viCam.pText);
                Console.WriteLine("Version API     : {0}", strText);
            }
        }

        static void Main(string[] args)
        {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
            {
                Console.WriteLine();
                PrintCameraInfoEx(0);
                Console.WriteLine();

                UnInitApi();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
