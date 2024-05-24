using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TUCAMAPI;

namespace C.NET18_gps_trigger
{
    class Program
    {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object
        public static TUCAM_FRAME m_frame;      // The frame object
        public static TUCAM_TRIGGER_ATTR m_tgr; // The trigger object
        public static TUCAM_VALUE_INFO m_valInfo;
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
                    Console.WriteLine("Please input 1 or 2");
                    int a = Console.Read();

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

        static void DoGPSTrigger()
        {

            TUCamAPI.TUCAM_Cap_GetTrigger(m_opCam.hIdxTUCam, ref m_tgr);
            m_tgr.nTgrMode = (int)TUCAM_CAPTURE_MODES.TUCCM_TRIGGER_GPS;
            m_tgr.nFrames = 1;
            m_tgr.nBufFrames = 1;
            TUCamAPI.TUCAM_Cap_SetTrigger(m_opCam.hIdxTUCam, m_tgr);

            int nTimes = 3;

            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            // Set GPS Parameter
            // GPS Start Time dwTime = (m_nHour << 16) + (m_nMin <<8) + m_nSec;
            TUCamAPI.TUCAM_Prop_SetValue(m_opCam.hIdxTUCam, (int)TUCAM_IDPROP.TUIDP_START_TIME, 0);

            // GPS Frame Number
            TUCamAPI.TUCAM_Prop_SetValue(m_opCam.hIdxTUCam, (int)TUCAM_IDPROP.TUIDP_FRAME_NUMBER, 1);

            // GPS Interval Time
            TUCamAPI.TUCAM_Prop_SetValue(m_opCam.hIdxTUCam, (int)TUCAM_IDPROP.TUIDP_INTERVAL_TIME, 1);

            // Get GPS parameter
            m_valInfo.nID = (int)TUCAM_IDINFO.TUIDI_UTCTIME;
            TUCamAPI.TUCAM_Dev_GetInfo(m_opCam.hIdxTUCam, ref m_valInfo);
            Console.WriteLine("UTC Reference Time: {0}", Marshal.PtrToStringAnsi(m_valInfo.pText));

            m_valInfo.nID = (int)TUCAM_IDINFO.TUIDI_LONGITUDE_LATITUDE;
            Console.WriteLine("UTC POS:{0}", Marshal.PtrToStringAnsi(m_valInfo.pText));


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
                        try 
	                    {	        
                            Marshal.Copy(buf, 0, handle.AddrOfPinnedObject(), (int)m_frame.usHeader);
	                    }
	                    catch (Exception)
	                    {
		
		                    throw;
	                    }
                        
                        
                        Console.WriteLine("Get the gps parameter success, index number is {0}, Year:{1}, Month:{2}, Day:{3}, Hour:{4}, Min:{5}, Sec:{6}, Ns:{7}",
                            i,
                            m_imgHeader.ucGPSTimeStampYear,
                            m_imgHeader.ucGPSTimeStampMonth,
                            m_imgHeader.ucGPSTimeStampDay,
                            m_imgHeader.ucGPSTimeStampHour,
                            m_imgHeader.ucGPSTimeStampMin,
                            m_imgHeader.ucGPSTimeStampSec,
                            m_imgHeader.nGPSTimeStampNs);
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
}
