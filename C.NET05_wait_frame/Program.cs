using System;
using System.Runtime.InteropServices;
using TUCAMAPI;

namespace C.NET05_wait_frame {
    class Program {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object
        public static TUCAM_FRAME m_frame;       // The frame object




        static void Main(string[] args) {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi()) {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0)) {
                    Console.WriteLine("Open the camera success");

                    Console.WriteLine();
                    WaitForImageData();
                    Console.WriteLine();

                    CloseCamera();
                }
                else {
                    Console.WriteLine("Open the camera failure");
                }

                UnInitApi();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        //This function is a blocking function example
        /* Wait for image data */
        static void WaitForImageData() {
            int nTimes = 10;

            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_SEQUENCE)) {
                for (int i = 0; i < nTimes; ++i) {
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(m_opCam.hIdxTUCam, ref m_frame)) {
                        Console.WriteLine("Grab the frame success, index number is {0}, width:{1}, height:{2}, channel:{3}, depth:{4}, image size:{5}", i, m_frame.usWidth, m_frame.usHeight, m_frame.ucChannels, (2 == m_frame.ucElemBytes) ? 16 : 8, m_frame.uiImgSize);
                    }
                    else {
                        Console.WriteLine("Grab the frame failure, index number is {0}", i);
                    }
                }

                TUCamAPI.TUCAM_Buf_AbortWait(m_opCam.hIdxTUCam);
                TUCamAPI.TUCAM_Cap_Stop(m_opCam.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_Release(m_opCam.hIdxTUCam);
        }

        #region Init & Open
        /* Init the TUCAM API */
        static TUCAMRET InitApi() {
            /* Get the current directory */
            IntPtr strPath = Marshal.StringToHGlobalAnsi(System.Environment.CurrentDirectory);

            m_itApi.uiCamCount = 0;
            m_itApi.pstrConfigPath = strPath;

            TUCamAPI.TUCAM_Api_Init(ref m_itApi);

            Console.WriteLine("Connect {0} camera", m_itApi.uiCamCount);

            if (0 == m_itApi.uiCamCount) {
                return TUCAMRET.TUCAMRET_NO_CAMERA;
            }

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        /* UnInit the TUCAM API */
        static TUCAMRET UnInitApi() {
            return TUCamAPI.TUCAM_Api_Uninit();
        }

        /* Open the camera by index number */
        static TUCAMRET OpenCamera(uint uiIdx) {
            if (uiIdx >= m_itApi.uiCamCount) {
                return TUCAMRET.TUCAMRET_OUT_OF_RANGE;
            }

            m_opCam.uiIdxOpen = uiIdx;

            return TUCamAPI.TUCAM_Dev_Open(ref m_opCam);
        }

        /* Close the current camera */
        static TUCAMRET CloseCamera() {
            if (null != m_opCam.hIdxTUCam) {
                TUCamAPI.TUCAM_Dev_Close(m_opCam.hIdxTUCam);
            }

            Console.WriteLine("Close the camera success");

            return TUCAMRET.TUCAMRET_SUCCESS;
        }
        #endregion


    }
}
