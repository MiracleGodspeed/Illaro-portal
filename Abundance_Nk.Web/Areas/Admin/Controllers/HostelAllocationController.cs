﻿using Abundance_Nk.Business;
using Abundance_Nk.Model.Model;
using Abundance_Nk.Web.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Abundance_Nk.Web.Controllers;
using Abundance_Nk.Web.Models;
using System.Transactions;
using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Microsoft.Ajax.Utilities;
using System.Configuration;

namespace Abundance_Nk.Web.Areas.Admin.Controllers
{
    public class HostelAllocationController : BaseController
    {
        private HostelViewModel viewModel;
        public HostelAllocationController()
        {
            viewModel = new HostelViewModel();
        }
        // GET: Admin/HostelAllocation
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult EditHostelRooms()
        {
            try
            {
                viewModel = new HostelViewModel();
                PopulateDropDownList();
            }
            catch (Exception ex)
            {
                SetMessage("Error! ", Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditHostelRooms(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();

                    List<RoomSetting> roomSettings = new List<RoomSetting>();
                    List<string> corners = new List<string>();

                    viewModel.HostelRoomList = hostelRoomLogic.GetModelsBy(hr => hr.Hostel_Id == viewModel.HostelRoom.Hostel.Id && hr.Series_Id == viewModel.HostelRoom.Series.Id);
                    if (viewModel.HostelRoomList.Count <= 0)
                    {
                        SetMessage("Rooms has not been created for this Hostel-Series combination", Message.Category.Error);
                        RetainDropDownList(viewModel);
                        return View(viewModel);
                    }
                    for (int i = 0; i < viewModel.HostelRoomList.Count; i++)
                    {
                        long roomId = viewModel.HostelRoomList[i].Id;
                        List<HostelRoomCorner> hostelRoomCornerList = hostelRoomCornerLogic.GetModelsBy(hrc => hrc.Room_Id == roomId);
                        if (hostelRoomCornerList != null)
                        {
                            for (int j = 0; j < hostelRoomCornerList.Count; j++)
                            {
                                if (!corners.Contains(hostelRoomCornerList[j].Name))
                                {
                                    corners.Add(hostelRoomCornerList[j].Name);
                                }
                            }

                            RoomSetting roomSetting = new RoomSetting();
                            roomSetting.HostelRoom = viewModel.HostelRoomList[i];
                            roomSetting.HostelRoomCorners = hostelRoomCornerList;

                            roomSettings.Add(roomSetting);
                        }
                    }

                    viewModel.Corners = corners;
                    viewModel.RoomSettings = roomSettings;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return View(viewModel);
        }
        public ActionResult SaveEditedRooms(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.HostelRoom != null && viewModel.RoomSettings.Count > 0)
                {
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    using (TransactionScope scope = new TransactionScope())
                    {
                        for (int i = 0; i < viewModel.RoomSettings.Count; i++)
                        {
                            long roomId = viewModel.RoomSettings[i].HostelRoom.Id;

                            HostelRoom hostelRoom = hostelRoomLogic.GetModelBy(hr => hr.Room_Id == roomId);
                            GeneralAudit generalAudit = new GeneralAudit();
                            generalAudit.Action = "Modify";
                            generalAudit.InitialValues = "Room Reserved = " + hostelRoom.Reserved + " Room Activated = " + hostelRoom.Activated;
                            hostelRoom.Number = viewModel.RoomSettings[i].HostelRoom.Number;
                            hostelRoom.Reserved = viewModel.RoomSettings[i].HostelRoom.Reserved;
                            hostelRoom.Activated = viewModel.RoomSettings[i].HostelRoom.Activated;
                            hostelRoom.Series = viewModel.HostelRoom.Series;
                            hostelRoom.Hostel = viewModel.HostelRoom.Hostel;

                            hostelRoomLogic.Modify(hostelRoom);
                            generalAudit.CurrentValues = "Room Reserved = " + viewModel.RoomSettings[i].HostelRoom.Reserved + " Room Activated = " + viewModel.RoomSettings[i].HostelRoom.Activated;
                            generalAudit.Operation = "Modified Room Number " + hostelRoom.Number + " in hostel" + hostelRoom.Hostel.Id;
                            generalAudit.TableNames = "Hostel Allocation";
                            generalAuditLogic.CreateGeneralAudit(generalAudit);
                            for (int j = 0; j < viewModel.RoomSettings[i].HostelRoomCorners.Count; j++)
                            {
                                long cornerId = viewModel.RoomSettings[i].HostelRoomCorners[j].Id;

                                HostelRoomCorner hostelRoomCorner = hostelRoomCornerLogic.GetModelBy(hrc => hrc.Corner_Id == cornerId);
                                generalAudit.InitialValues = " Room  Conner Activated = " + hostelRoomCorner.Activated;

                                hostelRoomCorner.Name = viewModel.RoomSettings[i].HostelRoomCorners[j].Name;
                                hostelRoomCorner.Activated = viewModel.RoomSettings[i].HostelRoomCorners[j].Activated;
                                hostelRoomCorner.Room = viewModel.RoomSettings[i].HostelRoom;
                                generalAudit.CurrentValues = " Room Conner Activated = " + viewModel.RoomSettings[i].HostelRoomCorners[j].Activated;
                                generalAudit.Operation = "Modified Room Conner " + hostelRoomCorner.Name + " in " + hostelRoom.Number + " in hostel ID " + hostelRoom.Hostel.Id;
                                generalAudit.TableNames = "Hostel Room Conner";

                                hostelRoomCornerLogic.Modify(hostelRoomCorner);
                                generalAuditLogic.CreateGeneralAudit(generalAudit);
                            }

                        }

                        SetMessage("Operation Successful! ", Message.Category.Information);

                        scope.Complete();
                    }
                }
                else
                {
                    SetMessage("Incomplete Input Elements Required To Perform This Operation", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("EditHostelRooms");
        }
        private void PopulateDropDownList()
        {
            try
            {
                ViewBag.HostelId = viewModel.HostelSelectListItem;
                ViewBag.HostelSeriesId = new SelectList(new List<HostelSeries>(), "Id", "Name");
                ViewBag.ProgrammeId = viewModel.ProgrammeSelectListItem;
                ViewBag.DepartmentId = new SelectList(new List<Department>(), "Id", "Name");
                ViewBag.LevelId = viewModel.LevelSelectListItem;
                ViewBag.CornerId = new MultiSelectList(new List<HostelRoomCorner>(), "Id", "Name");
                ViewBag.RoomId = new SelectList(new List<HostelRoom>(), "Id", "Name");

            }
            catch (Exception)
            {
                throw;
            }
        }
        private void RetainDropDownList(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    if (viewModel.HostelRoom != null)
                    {
                        if (viewModel.HostelRoom.Hostel != null)
                        {
                            ViewBag.HostelId = new SelectList(viewModel.HostelSelectListItem, "Value", "Text", viewModel.HostelRoom.Hostel.Id);
                        }
                        else
                        {
                            ViewBag.HostelId = viewModel.HostelSelectListItem;
                        }

                        if (viewModel.HostelRoom.Series != null && viewModel.HostelRoom.Hostel != null)
                        {
                            ViewBag.HostelSeriesId = new SelectList(Utility.PopulateHostelSeries(viewModel.HostelRoom.Hostel), "Value", "Text", viewModel.HostelRoom.Series.Id);
                        }
                        else
                        {
                            ViewBag.HostelSeriesId = new SelectList(new List<HostelSeries>(), "Id", "Name");
                        }
                    }

                    if (viewModel.HostelAllocationCriteria != null)
                    {
                        if (viewModel.HostelAllocationCriteria.Level != null)
                        {
                            ViewBag.LevelId = new SelectList(viewModel.LevelSelectListItem, "Value", "Text", viewModel.HostelAllocationCriteria.Level.Id);
                        }
                        else
                        {
                            ViewBag.LevelId = viewModel.LevelSelectListItem;
                        }

                        if (viewModel.HostelAllocationCriteria.Hostel != null)
                        {
                            ViewBag.HostelId = new SelectList(viewModel.HostelSelectListItem, "Value", "Text", viewModel.HostelAllocationCriteria.Hostel.Id);
                        }
                        else
                        {
                            ViewBag.HostelId = viewModel.HostelSelectListItem;
                        }

                        if (viewModel.HostelAllocationCriteria.Series != null && viewModel.HostelAllocationCriteria.Hostel != null)
                        {
                            ViewBag.HostelSeriesId = new SelectList(Utility.PopulateHostelSeries(viewModel.HostelAllocationCriteria.Hostel), "Value", "Text", viewModel.HostelAllocationCriteria.Series.Id);
                        }
                        else
                        {
                            ViewBag.HostelSeriesId = new SelectList(new List<HostelSeries>(), "Id", "Name");
                        }
                        if (viewModel.HostelAllocationCriteria.Corner != null && viewModel.HostelAllocationCriteria.Corner != null)
                        {
                            ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelAllocationCriteria.Room), "Value", "Text", viewModel.HostelAllocationCriteria.Corner.Id);
                        }
                        else
                        {
                            ViewBag.CornerId = new MultiSelectList(new List<HostelRoomCorner>(), "Id", "Name");
                        }
                        if (viewModel.HostelAllocationCriteria.Hostel != null && viewModel.HostelAllocationCriteria.Corner != null)
                        {
                            ViewBag.RoomId = new SelectList(Utility.PopulateHostelRooms(viewModel.HostelAllocationCriteria.Series), "Value", "Text", viewModel.HostelAllocationCriteria.Room.Id);
                        }
                        else
                        {

                            ViewBag.RoomId = new SelectList(new List<HostelRoom>(), "Id", "Name");
                        }
                    }

                    if (viewModel.HostelAllocation != null)
                    {

                        if (viewModel.HostelAllocation.Series != null && viewModel.HostelAllocation.Hostel != null)
                        {
                            ViewBag.HostelSeriesId = new SelectList(Utility.PopulateHostelSeries(viewModel.HostelAllocation.Hostel), "Value", "Text", viewModel.HostelAllocation.Series.Id);
                        }
                        else
                        {
                            ViewBag.HostelSeriesId = new SelectList(new List<HostelSeries>(), "Id", "Name");
                        }
                        if (viewModel.HostelAllocation.Corner != null && viewModel.HostelAllocation.Corner != null)
                        {
                            ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelAllocation.Room), "Value", "Text", viewModel.HostelAllocation.Corner.Id);
                        }
                        else
                        {
                            ViewBag.CornerId = new MultiSelectList(new List<HostelRoomCorner>(), "Id", "Name");
                        }
                        if (viewModel.HostelAllocation.Hostel != null && viewModel.HostelAllocation.Corner != null)
                        {
                            ViewBag.RoomId = new SelectList(Utility.PopulateHostelRooms(viewModel.HostelAllocation.Series), "Value", "Text", viewModel.HostelAllocation.Room.Id);
                        }
                        else
                        {

                            ViewBag.RoomId = new SelectList(new List<HostelRoom>(), "Id", "Name");
                        }
                        if (viewModel.HostelAllocation.Hostel != null)
                        {
                            ViewBag.HostelId = new SelectList(viewModel.HostelSelectListItem, "Value", "Text", viewModel.HostelAllocation.Hostel.Id);
                        }
                        else
                        {
                            ViewBag.HostelId = viewModel.HostelSelectListItem;
                        }

                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public ActionResult CreateHostelRooms()
        {
            try
            {
                viewModel = new HostelViewModel();
                PopulateDropDownList();
            }
            catch (Exception ex)
            {
                SetMessage("Error! ", Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult CreateHostelRooms(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    HostelSeriesLogic hostelSeriesLogic = new HostelSeriesLogic();
                    HostelLogic hostelLogic = new HostelLogic();

                    HostelSeries hostelSeries = hostelSeriesLogic.GetModelBy(hs => hs.Series_Id == viewModel.HostelRoom.Series.Id);
                    Hostel hostel = hostelLogic.GetModelBy(h => h.Hostel_Id == viewModel.HostelRoom.Hostel.Id);

                    if (hostelSeries == null)
                    {
                        SetMessage("Select the hostel Series", Message.Category.Error);
                        RetainDropDownList(viewModel);
                        return View(viewModel);
                    }

                    int roomCapacity = Convert.ToInt32(viewModel.HostelRoom.RoomCapacity);
                    int coners = Convert.ToInt32(viewModel.HostelRoom.Corners);

                    List<RoomSetting> allRoomSettings = new List<RoomSetting>();

                    for (int i = 0; i < roomCapacity; i++)
                    {
                        HostelRoom hostelRoom = new HostelRoom();
                        hostelRoom.Hostel = hostel;
                        hostelRoom.Series = hostelSeries;
                        hostelRoom.Activated = true;
                        hostelRoom.Reserved = false;
                        hostelRoom.Number = "Room " + (i + 1).ToString();

                        //if (hostelSeries.Id == 1)
                        //{
                        //    hostelRoom.Number = (100 + i).ToString();
                        //}
                        //if (hostelSeries.Id == 2)
                        //{
                        //    hostelRoom.Number = (200 + i).ToString();
                        //}
                        //if (hostelSeries.Id == 3)
                        //{
                        //    hostelRoom.Number = (300 + i).ToString();
                        //}
                        //if (hostelSeries.Id == 4)
                        //{
                        //    hostelRoom.Number = (400 + i).ToString();
                        //}

                        List<HostelRoomCorner> hostelRoomCorners = new List<HostelRoomCorner>();
                        for (int j = 0; j < coners; j++)
                        {
                            HostelRoomCorner hostelRoomCorner = new HostelRoomCorner();
                            hostelRoomCorner.Activated = true;
                            hostelRoomCorner.Name = GetCornerName(j).Trim();

                            hostelRoomCorners.Add(hostelRoomCorner);
                        }

                        allRoomSettings.Add(new RoomSetting() { HostelRoom = hostelRoom, HostelRoomCorners = hostelRoomCorners });
                    }

                    viewModel.RoomSettings = allRoomSettings;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! ", Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return View(viewModel);
        }
        public ActionResult SaveRooms(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.HostelRoom != null && viewModel.RoomSettings.Count > 0)
                {
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    List<HostelRoom> hostelRooms = hostelRoomLogic.GetModelsBy(hr => hr.Hostel_Id == viewModel.HostelRoom.Hostel.Id && hr.Series_Id == viewModel.HostelRoom.Series.Id);

                    using (TransactionScope scope = new TransactionScope())
                    {
                        for (int i = 0; i < viewModel.RoomSettings.Count; i++)
                        {
                            HostelRoom hostelRoomCheck = hostelRooms.Where(hr => hr.Number == viewModel.RoomSettings[i].HostelRoom.Number).FirstOrDefault();
                            if (hostelRoomCheck == null)
                            {
                                HostelRoom hostelRoom = new HostelRoom();
                                hostelRoom.Number = viewModel.RoomSettings[i].HostelRoom.Number;
                                hostelRoom.Reserved = viewModel.RoomSettings[i].HostelRoom.Reserved;
                                hostelRoom.Activated = viewModel.RoomSettings[i].HostelRoom.Activated;
                                hostelRoom.Series = viewModel.HostelRoom.Series;
                                hostelRoom.Hostel = viewModel.HostelRoom.Hostel;

                                HostelRoom newHostelRoom = hostelRoomLogic.Create(hostelRoom);
                                string Action = "CREATE";
                                string Operation = "Created Hostel Room " + hostelRoom.Number;
                                string Table = "Hostel Room";
                                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);



                                for (int j = 0; j < viewModel.RoomSettings[i].HostelRoomCorners.Count; j++)
                                {
                                    HostelRoomCorner hostelRoomCorner = new HostelRoomCorner();
                                    hostelRoomCorner.Name = viewModel.RoomSettings[i].HostelRoomCorners[j].Name;
                                    hostelRoomCorner.Activated = viewModel.RoomSettings[i].HostelRoomCorners[j].Activated;
                                    hostelRoomCorner.Room = newHostelRoom;

                                    hostelRoomCornerLogic.Create(hostelRoomCorner);
                                    string Action2 = "CREATE";
                                    string Operation2 = "Created Hostel Room Conner  " + hostelRoomCorner.Name + " for " + hostelRoom.Number;
                                    string Table2 = "Hostel Room Conner ";
                                    generalAuditLogic.CreateGeneralAudit(Action2, Operation2, Table2);


                                }
                            }
                        }

                        SetMessage("Operation Successful! ", Message.Category.Information);

                        scope.Complete();
                    }
                }
                else
                {
                    SetMessage("Incomplete Input Elements Required To Perform This Operation", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("CreateHostelRooms");
        }
        public ActionResult CreateHostelAllocationCriteria()
        {
            try
            {
                viewModel = new HostelViewModel();
                PopulateDropDownList();
            }
            catch (Exception ex)
            {
                SetMessage("Error! ", Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult CreateHostelAllocationCriteria(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.HostelAllocationCriteria.Series != null && viewModel.SelectedCorners.Any())
                {
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                    HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();
                    HostelAllocationCriteria hostelAllocationCriteria = new HostelAllocationCriteria();
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    hostelAllocationCriteria.Hostel = viewModel.HostelAllocationCriteria.Hostel;
                    hostelAllocationCriteria.Level = viewModel.HostelAllocationCriteria.Level;
                    hostelAllocationCriteria.Series = viewModel.HostelAllocationCriteria.Series;

                    List<HostelRoom> hostelRooms = hostelRoomLogic.GetModelsBy(hr => hr.Series_Id == viewModel.HostelAllocationCriteria.Series.Id && hr.Activated);

                    using (TransactionScope scope = new TransactionScope())
                    {
                        for (int i = 0; i < hostelRooms.Count; i++)
                        {
                            hostelAllocationCriteria.Room = hostelRooms[i];
                            for (int j = 0; j < viewModel.SelectedCorners.Count(); j++)
                            {
                                long roomId = hostelRooms[i].Id;
                                string cornerName = viewModel.SelectedCorners[j];
                                HostelRoomCorner hostelRoomCorner = hostelRoomCornerLogic.GetModelBy(hrc => hrc.Room_Id == roomId && hrc.Corner_Name == cornerName && hrc.Activated);
                                if (hostelRoomCorner != null)
                                {
                                    hostelAllocationCriteria.Corner = hostelRoomCorner;

                                    HostelAllocationCriteria existingCriteria = hostelAllocationCriteriaLogic.GetModelBy(hac => hac.Corner_Id == hostelRoomCorner.Id && hac.Hostel_Id == viewModel.HostelAllocationCriteria.Hostel.Id && hac.Level_Id == viewModel.HostelAllocationCriteria.Level.Id && hac.Room_Id == roomId && hac.Series_Id == viewModel.HostelAllocationCriteria.Series.Id);
                                    if (existingCriteria == null)
                                    {
                                        hostelAllocationCriteriaLogic.Create(hostelAllocationCriteria);
                                        string Action = "CREATE";
                                        string Operation = "Created Hostel Criteria for " + hostelRooms[i].Number + " Level = " + hostelAllocationCriteria.Level.Name + " Series = " + hostelAllocationCriteria.Series.Name + " for Hostel " + hostelAllocationCriteria.Hostel.Name;
                                        string Table = "Hostel Allocation Criteria";
                                        generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);


                                    }
                                }

                            }
                        }

                        scope.Complete();
                        SetMessage("Operation Successful!", Message.Category.Information);
                        return RedirectToAction("CreateHostelAllocationCriteria");
                    }
                }
                else
                {
                    SetMessage("Inadequate parameters required to service operation!", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! ", Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return View(viewModel);
        }

        public ActionResult ViewHostelAllocationCriteria()
        {
            try
            {
                viewModel = new HostelViewModel();
                PopulateDropDownList();
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewHostelAllocationCriteria(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    HostelAllocationCriteriaLogic criteriaLogic = new HostelAllocationCriteriaLogic();
                    List<HostelAllocationCriteria> allocationCriterias = new List<HostelAllocationCriteria>();
                    allocationCriterias = criteriaLogic.GetModelsBy(h => h.Level_Id == viewModel.HostelAllocationCriteria.Level.Id);

                    viewModel.HostelAllocationCriterias = allocationCriterias;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error Occured! " + ex.Message, Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return View(viewModel);
        }

        public ActionResult EditHostelAllocationCriteria(int hId)
        {
            try
            {
                viewModel = new HostelViewModel();
                HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                HostelAllocationCriteria criteria = hostelAllocationCriteriaLogic.GetModelBy(x => x.Id == hId);
                if (criteria != null)
                {
                    viewModel.HostelAllocationCriteria = criteria;
                    RetainDropDownList(viewModel);
                    //TempData["HostelAllocationViewModel"] = viewModel;
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditHostelAllocationCriteria(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.HostelAllocationCriteria != null)
                {
                    HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                    HostelAllocationCriteria existingCriteria = hostelAllocationCriteriaLogic.GetModelBy(hac => hac.Corner_Id == viewModel.HostelAllocationCriteria.Corner.Id && hac.Hostel_Id == viewModel.HostelAllocationCriteria.Hostel.Id && hac.Level_Id == viewModel.HostelAllocationCriteria.Level.Id && hac.Room_Id == viewModel.HostelAllocationCriteria.Room.Id && hac.Series_Id == viewModel.HostelAllocationCriteria.Series.Id);
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                    if (viewModel.HostelAllocationCriteria.EditAll)
                    {
                        List<HostelAllocationCriteria> existingCriteriaList = hostelAllocationCriteriaLogic.GetModelsBy(h => h.Hostel_Id == viewModel.HostelAllocationCriteria.Hostel.Id && h.Series_Id == viewModel.HostelAllocationCriteria.Series.Id && h.Level_Id == viewModel.HostelAllocationCriteria.Level.Id);
                        for (int i = 0; i < existingCriteriaList.Count; i++)
                        {
                            HostelAllocationCriteria currentCriteria = existingCriteriaList[i];
                            currentCriteria.Level = viewModel.Level;

                            hostelAllocationCriteriaLogic.Modify(currentCriteria);
                            GeneralAudit generalAudit = new GeneralAudit();
                            generalAudit.Action = "Modify";
                            generalAudit.InitialValues = "Level ID = " + existingCriteriaList[i].Level.Id;
                            generalAudit.CurrentValues = "Level ID = " + viewModel.Level.Id;
                            generalAudit.Operation = "Edited Hostel Allocation Creteria for hostel ID " + currentCriteria.Hostel.Id;
                            generalAudit.TableNames = "Hostel Allocation Creteria";
                            generalAuditLogic.CreateGeneralAudit(generalAudit);



                        }

                        SetMessage("Operation Successful ", Message.Category.Information);
                        return RedirectToAction("ViewHostelAllocationCriteria");
                    }
                    else if (!viewModel.HostelAllocationCriteria.EditAll)
                    {
                        if (existingCriteria == null)
                        {
                            viewModel.HostelAllocationCriteria.Level = viewModel.Level;
                            hostelAllocationCriteriaLogic.Modify(viewModel.HostelAllocationCriteria);
                            GeneralAudit generalAudit = new GeneralAudit();
                            generalAudit.Action = "Modify";
                            generalAudit.InitialValues = "Level ID = " + existingCriteria.Level.Id;
                            generalAudit.CurrentValues = "Level ID = " + viewModel.Level.Id;
                            generalAudit.Operation = "Edited Hostel Allocation Creteria for hostel ID " + viewModel.HostelAllocationCriteria.Hostel.Id;
                            generalAudit.TableNames = "Hostel Allocation Creteria";
                            generalAuditLogic.CreateGeneralAudit(generalAudit);

                            SetMessage("Operation Successful ", Message.Category.Information);
                            return RedirectToAction("ViewHostelAllocationCriteria");
                        }
                        else
                        {
                            SetMessage("Error! Criteria exists/NO Changes Made.", Message.Category.Information);

                            RetainDropDownList(viewModel);
                            return View();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return RedirectToAction("ViewHostelAllocationCriteria");
        }

        public ActionResult ConfirmDeleteHostelAllocationCriteria(int hid)
        {
            try
            {
                viewModel = new HostelViewModel();
                if (hid > 0)
                {
                    HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                    viewModel.HostelAllocationCriteria = hostelAllocationCriteriaLogic.GetModelBy(x => x.Id == hid);

                    RetainDropDownList(viewModel);
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult DeleteHostelAllocationCriteria(HostelViewModel viewModel)
        {
            try
            {
                HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                hostelAllocationCriteriaLogic.Delete(x => x.Id == viewModel.HostelAllocationCriteria.Id);
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                GeneralAudit generalAudit = new GeneralAudit();
                generalAudit.Action = "Delete";
                generalAudit.InitialValues = "Level ID = " + viewModel.HostelAllocationCriteria.Level.Id + " For Hostel " + viewModel.HostelAllocationCriteria.Hostel.Id;
                generalAudit.CurrentValues = "-";
                generalAudit.Operation = "Deleted Hostel Allocation Creteria for hostel ID " + viewModel.HostelAllocationCriteria.Hostel.Id;
                generalAudit.TableNames = "Hostel Allocation Creteria";
                generalAuditLogic.CreateGeneralAudit(generalAudit);

                SetMessage("Operation Successful!", Message.Category.Information);
                return RedirectToAction("ViewHostelAllocationCriteria");

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View("ConfirmDeleteHostelAllocationCriteria", viewModel);
        }
        public JsonResult RemoveCriteria(string hid)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                long criteriaId = Convert.ToInt64(hid);
                var getHostel = hostelAllocationCriteriaLogic.GetModelBy(h => h.Id == criteriaId);
                hostelAllocationCriteriaLogic.Delete(x => x.Id == criteriaId);
                string Action = "DELETE";
                string Operation = "Deleted Hostel Criteria for level ID " + getHostel.Level.Id + " For Hostel Id = " + getHostel.Hostel.Id + " For Room  = " + getHostel.Room.Number;
                string Table = "Hostel Allocation Criteria";
                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                result.IsError = false;
                result.Message = "Operation Successful!";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = "Error! " + ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveAllCriteria()
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();

                hostelAllocationCriteriaLogic.Delete(x => x.Id > 0);
                string Action = "DELETE";
                string Operation = "Deleted all Hostel Criteria for ";
                string Table = "Hostel Allocation Criteria";
                generalAuditLogic.CreateGeneralAudit(Action, Operation, Table);

                result.IsError = false;
                result.Message = "Operation Successful!";
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = "Error! " + ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditStudentHostelAllocation()
        {
            try
            {
                viewModel = new HostelViewModel();
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult EditStudentHostelAllocation(HostelViewModel viewModel)
        {
            try
            {
                Model.Model.Student student = new Model.Model.Student();
                StudentLevel studentLevel = new StudentLevel();
                HostelAllocation hostelAllocation = new HostelAllocation();

                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                StudentLogic studentLogic = new StudentLogic();
                SessionLogic sessionLogic = new SessionLogic();

                List<StudentLevel> studentLevels = new List<StudentLevel>();

                List<Model.Model.Student> students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.Student.MatricNumber);
                if (students.Count != 1)
                {
                    SetMessage("Student with this Matriculation Number does not exist Or Matric Number is Duplicate!", Message.Category.Error);
                    return View(viewModel);
                }

                student = students.FirstOrDefault();
                studentLevels = studentLevelLogic.GetModelsBy(sl => sl.STUDENT.Person_Id == student.Id);
                if (studentLevels.Count == 0)
                {
                    SetMessage("No StudentLevel Record!", Message.Category.Error);
                    return View(viewModel);
                }

                int maxLevelId = studentLevels.Max(sl => sl.Level.Id);
                studentLevel = studentLevels.Where(sl => sl.Level.Id == maxLevelId).LastOrDefault();
                viewModel.StudentLevel = studentLevel;
                viewModel.Session = sessionLogic.GetHostelSession();
                //viewModel.Session = new Session() { Id = (int)Sessions._20172018 };

                hostelAllocation = hostelAllocationLogic.GetModelBy(x => x.Student_Id == student.Id && x.Session_Id == viewModel.Session.Id);
                if (hostelAllocation != null)
                {
                    viewModel.HostelAllocation = hostelAllocation;
                    RetainDropDownList(viewModel);
                    return View(viewModel);
                }
                else
                {
                    SetMessage("No Hostel Allocation for this Student in the current session", Message.Category.Error);
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        public ActionResult SaveEditedStudentHostelAllocation(HostelViewModel viewModel)
        {
            try
            {
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();
                HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                HostelAllocation hostelAllocation = new HostelAllocation();
                HostelRoomCorner corner = new HostelRoomCorner();
                HostelRoom hostelRoom = new HostelRoom();
                GeneralAudit generalAudit = new GeneralAudit();

                if (viewModel.HostelAllocation.Series != null && viewModel.HostelAllocation.Room != null && viewModel.HostelAllocation.Hostel != null && viewModel.HostelAllocation.Corner != null)
                {
                    HostelAllocation existingAllocation = hostelAllocationLogic.GetModelBy(
                                                            x =>
                                                            x.Hostel_Id == viewModel.HostelAllocation.Hostel.Id &&
                                                            x.Series_Id == viewModel.HostelAllocation.Series.Id
                                                            && x.Room_Id == viewModel.HostelAllocation.Room.Id &&
                                                            x.Corner_Id == viewModel.HostelAllocation.Corner.Id &&
                                                            x.Session_Id == viewModel.HostelAllocation.Session.Id
                                                            && x.Occupied);
                    if (existingAllocation != null)
                    {
                        SetMessage("The Room and corner you are trying to allocate is Occupied! ", Message.Category.Error);
                        RetainDropDownList(viewModel);
                        return RedirectToAction("EditStudentHostelAllocation");
                    }

                    hostelRoom = hostelRoomLogic.GetModelBy(h => h.Room_Id == viewModel.HostelAllocation.Room.Id);
                    corner = hostelRoomCornerLogic.GetModelBy(h => h.Corner_Id == viewModel.HostelAllocation.Corner.Id);
                    if (hostelRoom.Reserved || !corner.Activated)
                    {
                        SetMessage("The room and corner you are trying to allocate is reserved or already allocated to another student! ", Message.Category.Error);
                        RetainDropDownList(viewModel);
                        return RedirectToAction("EditStudentHostelAllocation");
                    }


                    generalAudit.Action = "Modify";
                    generalAudit.InitialValues = "Room Number = " + hostelRoom.Number + " Room Conner = " + corner.Name;

                    hostelAllocation.Id = viewModel.HostelAllocation.Id;
                    hostelAllocation.Hostel = viewModel.HostelAllocation.Hostel;
                    hostelAllocation.Series = viewModel.HostelAllocation.Series;
                    hostelAllocation.Room = viewModel.HostelAllocation.Room;
                    hostelAllocation.Corner = viewModel.HostelAllocation.Corner;
                    generalAudit.CurrentValues = " Room Number = " + viewModel.HostelAllocation.Room.Number + " Room Conner = " + viewModel.HostelAllocation.Corner.Name;
                    generalAudit.Operation = "Modified Room and Conner for Hostel Allocation Id " + hostelAllocation.Id;
                    generalAudit.TableNames = "Hostel Room Conner";



                }

                bool IsModified = hostelAllocationLogic.Modify(hostelAllocation);
                if (IsModified)
                {
                    generalAuditLogic.CreateGeneralAudit(generalAudit);

                    SetMessage("Operation Successful!", Message.Category.Information);
                }
                else
                {
                    SetMessage("NO Changes Made!", Message.Category.Error);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("EditStudentHostelAllocation");
        }

        public ActionResult ChangeRoomName()
        {
            try
            {
                viewModel = new HostelViewModel();
                PopulateDropDownList();
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ChangeRoomName(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    viewModel.HostelRoom = hostelRoomLogic.GetModelBy(h => h.Hostel_Id == viewModel.HostelAllocation.Hostel.Id && h.Series_Id == viewModel.HostelAllocation.Series.Id && h.Room_Id == viewModel.HostelAllocation.Room.Id);

                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return View(viewModel);
        }
        public ActionResult SaveChangedRoomName(HostelViewModel viewModel)
        {
            try
            {
                GeneralAudit generalAudit = new GeneralAudit();
                GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                if (viewModel.HostelRoom.Number != null)
                {
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();

                    HostelRoom hostelRoom = hostelRoomLogic.GetModelBy(h => h.Room_Id == viewModel.HostelRoom.Id);
                    generalAudit.Action = "Modify";
                    generalAudit.InitialValues = "Room Number = " + hostelRoom.Number + " Room Name = ";

                    hostelRoom.Number = viewModel.HostelRoom.Number;

                    generalAudit.CurrentValues = " Room Number = " + viewModel.HostelRoom.Number;
                    generalAudit.Operation = "Modified Room and Conner for Hostel Id " + hostelRoom.Hostel.Id;
                    generalAudit.TableNames = "Hostel";

                    hostelRoomLogic.Modify(hostelRoom);
                    generalAuditLogic.CreateGeneralAudit(generalAudit);
                    SetMessage("Operation! Successful! ", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ChangeRoomName");
        }

        public ActionResult ViewUnoccupiedAllocations()
        {
            try
            {
                viewModel = new HostelViewModel();
                ViewBag.Session = viewModel.SessionSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewUnoccupiedAllocations(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.Session != null)
                {
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    viewModel.HostelAllocations = hostelAllocationLogic.GetModelsBy(h => h.Session_Id == viewModel.Session.Id && h.Occupied == false);
                    StudentLogic studentLogic = new StudentLogic();
                    for (int i = 0; i < viewModel.HostelAllocations.Count; i++)
                    {
                        HostelAllocation currentAllocation = viewModel.HostelAllocations[i];
                        if (viewModel.HostelAllocations[i].Person != null)
                        {
                            viewModel.HostelAllocations[i].Student = studentLogic.GetModelsBy(s => s.Person_Id == currentAllocation.Person.Id).LastOrDefault();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Session = viewModel.SessionSelectListItem;
            return View(viewModel);
        }
        public ActionResult ViewAllAllocations()
        {
            try
            {
                viewModel = new HostelViewModel();
                ViewBag.Session = viewModel.SessionSelectListItem;
                ViewBag.Hostel = viewModel.HostelSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewAllAllocations(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.Session != null && viewModel.Hostel != null)
                {
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    viewModel.HostelAllocations = hostelAllocationLogic.GetModelsBy(h => h.Session_Id == viewModel.Session.Id && h.Hostel_Id == viewModel.Hostel.Id);
                    StudentLogic studentLogic = new StudentLogic();
                    for (int i = 0; i < viewModel.HostelAllocations.Count; i++)
                    {
                        HostelAllocation currentAllocation = viewModel.HostelAllocations[i];
                        if (viewModel.HostelAllocations[i].Person != null)
                        {
                            viewModel.HostelAllocations[i].Student = studentLogic.GetModelsBy(s => s.Person_Id == currentAllocation.Person.Id).LastOrDefault();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Session = viewModel.SessionSelectListItem;
            ViewBag.Hostel = viewModel.HostelSelectListItem;
            return View(viewModel);
        }
        public ActionResult DeleteUnoccupiedAllocations(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.HostelAllocations.Count > 0)
                {
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    HostelAllocationCountLogic hostelAllocationCountLogic = new HostelAllocationCountLogic();
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    //PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    //StudentPaymentLogic studentPaymentLogic = new StudentPaymentLogic();
                    RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                    RemitaResponse remitaResponse = new RemitaResponse();
                    RemitaSettings settings = new RemitaSettings();
                    settings = settingsLogic.GetModelBy(s => s.Payment_SettingId == 1);
                    string remitaVerifyUrl = ConfigurationManager.AppSettings["RemitaVerifyUrl"].ToString();
                    RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);

                    StudentLevel studentLevel = new StudentLevel();

                    List<HostelAllocation> allocations = viewModel.HostelAllocations.Where(h => h.Occupied).ToList();

                    for (int i = 0; i < allocations.Count; i++)
                    {
                        HostelAllocation currentAllocation = allocations[i];
                        currentAllocation = hostelAllocationLogic.GetModelBy(h => h.Id == currentAllocation.Id);

                        //PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(p => p.Payment_Id == currentAllocation.Payment.Id);
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetModelBy(r => r.Payment_Id == currentAllocation.Payment.Id && r.Status.Contains("01"));
                        if (remitaPayment != null)
                        {
                            currentAllocation.Occupied = true;
                            hostelAllocationLogic.Modify(currentAllocation);

                            continue;
                        }

                        remitaResponse = remitaProcessor.TransactionStatus(remitaVerifyUrl, remitaPaymentLogic.GetModelBy(r => r.Payment_Id == currentAllocation.Payment.Id));
                        if (remitaResponse != null && remitaResponse.Status != null && remitaResponse.Status.Contains("01"))
                        {
                            remitaPayment.Status = remitaResponse.Status + ":" + remitaResponse.StatusCode;
                            remitaPaymentLogic.Modify(remitaPayment);

                            currentAllocation.Occupied = true;
                            hostelAllocationLogic.Modify(currentAllocation);

                            continue;
                        }

                        int levelId = 1;

                        HostelRequestLogic requestLogic = new HostelRequestLogic();
                        HostelRequest hostelRequest = requestLogic.GetModelsBy(r => r.Person_Id == currentAllocation.Person.Id && r.Session_Id == currentAllocation.Session.Id).LastOrDefault();
                        if (hostelRequest != null)
                            levelId = hostelRequest.Level.Id;

                        using (TransactionScope transactionScope = new TransactionScope())
                        {
                            hostelAllocationLogic.Delete(h => h.Id == currentAllocation.Id);

                            onlinePaymentLogic.Delete(o => o.Payment_Id == currentAllocation.Payment.Id);

                            remitaPaymentLogic.Delete(o => o.Payment_Id == currentAllocation.Payment.Id);

                            hostelFeeLogic.Delete(h => h.Payment_Id == currentAllocation.Payment.Id);

                            AddStudentToBlackList(currentAllocation);

                            hostelRequestLogic.Delete(h => h.Person_Id == currentAllocation.Person.Id && h.Session_Id == currentAllocation.Session.Id);

                            //studentPaymentLogic.Delete(s => s.Payment_Id == currentAllocation.Payment.Id);

                            paymentLogic.Delete(p => p.Payment_Id == currentAllocation.Payment.Id);

                            HostelRoom hostelRoom = hostelRoomLogic.GetModelBy(h => h.Room_Id == currentAllocation.Room.Id);

                            if (hostelRoom != null && hostelRoom.Reserved)
                            {
                                studentLevel = studentLevelLogic.GetModelBy(s => s.Session_Id == currentAllocation.Session.Id && s.Person_Id == currentAllocation.Person.Id);
                                if (studentLevel != null)
                                {
                                    HostelAllocationCount hostelAllocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Level_Id == studentLevel.Level.Id && h.Sex_Id == currentAllocation.Person.Sex.Id);
                                    if (hostelAllocationCount != null)
                                    {
                                        hostelAllocationCount.TotalCount += 1;
                                        hostelAllocationCount.Reserved += 1;

                                        hostelAllocationCountLogic.Modify(hostelAllocationCount);
                                    }
                                }
                                else
                                {
                                    HostelAllocationCount hostelAllocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Level_Id == levelId && h.Sex_Id == currentAllocation.Person.Sex.Id);
                                    if (hostelAllocationCount != null)
                                    {
                                        hostelAllocationCount.TotalCount += 1;
                                        hostelAllocationCount.Reserved += 1;

                                        hostelAllocationCountLogic.Modify(hostelAllocationCount);
                                    }
                                }
                            }
                            else if (hostelRoom != null && !hostelRoom.Reserved)
                            {
                                studentLevel = studentLevelLogic.GetModelBy(s => s.Session_Id == currentAllocation.Session.Id && s.Person_Id == currentAllocation.Person.Id);
                                if (studentLevel != null)
                                {
                                    HostelAllocationCount hostelAllocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Level_Id == studentLevel.Level.Id && h.Sex_Id == currentAllocation.Person.Sex.Id);
                                    if (hostelAllocationCount != null)
                                    {
                                        hostelAllocationCount.TotalCount += 1;
                                        hostelAllocationCount.Free += 1;

                                        hostelAllocationCountLogic.Modify(hostelAllocationCount);
                                    }
                                }
                                else
                                {
                                    HostelAllocationCount hostelAllocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Level_Id == levelId && h.Sex_Id == currentAllocation.Person.Sex.Id);
                                    if (hostelAllocationCount != null)
                                    {
                                        hostelAllocationCount.TotalCount += 1;
                                        hostelAllocationCount.Free += 1;

                                        hostelAllocationCountLogic.Modify(hostelAllocationCount);
                                    }
                                }
                            }

                            transactionScope.Complete();
                        }
                    }

                    SetMessage("Operation! Successful! ", Message.Category.Information);
                    return RedirectToAction("ViewUnoccupiedAllocations");
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Session = viewModel.SessionSelectListItem;
            return View("ViewUnoccupiedAllocations");
        }

        private void AddStudentToBlackList(HostelAllocation currentAllocation)
        {
            try
            {
                HostelBlacklistLogic hostelBlacklistLogic = new HostelBlacklistLogic();
                StudentLevelLogic studentLevelLogic = new StudentLevelLogic();

                StudentLevel studentLevel = studentLevelLogic.GetModelsBy(s => s.Person_Id == currentAllocation.Person.Id && s.Session_Id == currentAllocation.Session.Id).LastOrDefault();
                if (studentLevel != null)
                {
                    HostelBlacklist hostelBlacklist = new HostelBlacklist();
                    hostelBlacklist.Department = studentLevel.Department;
                    hostelBlacklist.Level = studentLevel.Level;
                    hostelBlacklist.Programme = studentLevel.Programme;
                    hostelBlacklist.Reason = "Delayed Payment";
                    hostelBlacklist.Session = currentAllocation.Session;
                    hostelBlacklist.Student = currentAllocation.Student;

                    if (hostelBlacklist.Student != null)
                    {
                        hostelBlacklistLogic.Create(hostelBlacklist);
                    }
                }
                else
                {
                    AppliedCourseLogic appliedCourseLogic = new AppliedCourseLogic();
                    AppliedCourse appliedCourse = appliedCourseLogic.GetModelsBy(a => a.Person_Id == currentAllocation.Person.Id).LastOrDefault();

                    if (appliedCourse != null)
                    {
                        HostelBlacklist hostelBlacklist = new HostelBlacklist();
                        hostelBlacklist.Department = appliedCourse.Department;
                        hostelBlacklist.Level = new Level() { Id = appliedCourse.Programme.Id > 2 ? 3 : 1 };
                        hostelBlacklist.Programme = appliedCourse.Programme;
                        hostelBlacklist.Reason = "Delayed Payment";
                        hostelBlacklist.Session = currentAllocation.Session;
                        hostelBlacklist.Student = currentAllocation.Student;

                        if (hostelBlacklist.Student != null)
                        {
                            hostelBlacklistLogic.Create(hostelBlacklist);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult ViewReservedRooms()
        {
            try
            {
                viewModel = new HostelViewModel();
                PopulateDropDownList();
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);

        }
        [HttpPost]
        public ActionResult ViewReservedRooms(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    viewModel.HostelRoomList = hostelRoomLogic.GetModelsBy(h => h.Hostel_Id == viewModel.HostelAllocation.Hostel.Id && h.Series_Id == viewModel.HostelAllocation.Series.Id && h.Reserved);

                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            RetainDropDownList(viewModel);
            return View(viewModel);

        }
        public ActionResult ReleaseReservedRooms(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.HostelRoomList.Count > 0)
                {
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    List<HostelRoom> roomsToRelease = viewModel.HostelRoomList.Where(r => r.Reserved == false).ToList();

                    for (int i = 0; i < roomsToRelease.Count; i++)
                    {
                        HostelRoom currentHostelRoom = roomsToRelease[i];
                        HostelRoom hostelRoom = hostelRoomLogic.GetModelBy(h => h.Room_Id == currentHostelRoom.Id);
                        hostelRoom.Reserved = currentHostelRoom.Reserved;
                        hostelRoomLogic.Modify(hostelRoom);

                        SetMessage("Operation! Successful! ", Message.Category.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewReservedRooms");
        }
        public ActionResult AllocateReservedRoom(int rid)
        {
            try
            {
                if (rid > 0)
                {
                    HostelViewModel viewModel = new HostelViewModel();
                    HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();
                    HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();

                    viewModel.HostelRoom = hostelRoomLogic.GetModelBy(h => h.Room_Id == rid);
                    viewModel.HostelRoomCorners = hostelRoomCornerLogic.GetModelsBy(c => c.Room_Id == rid);

                    ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelRoom), "Value", "Text");
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }


            return RedirectToAction("ViewReservedRooms");
        }
        [HttpPost]
        public ActionResult AllocateReservedRoom(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel != null)
                {
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    StudentLogic studentLogic = new StudentLogic();
                    PersonLogic personLogic = new PersonLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    HostelAllocationCountLogic hostelAllocationCountLogic = new HostelAllocationCountLogic();
                    SessionLogic sessionLogic = new SessionLogic();

                    HostelAllocation hostelAllocation = new HostelAllocation();
                    Payment craetedPayment = new Payment();

                    List<Model.Model.Student> students = studentLogic.GetModelsBy(s => s.Matric_Number == viewModel.Student.MatricNumber);
                    if (students.Count != 1)
                    {
                        SetMessage("Matric Number is duplicate Or doesn't exist", Message.Category.Error);
                        ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelRoom), "Value", "Text");
                        return View(viewModel);
                    }

                    Model.Model.Student currentStudent = students.FirstOrDefault();
                    Person person = personLogic.GetModelBy(s => s.Person_Id == currentStudent.Id);
                    List<StudentLevel> studentLevels = studentLevelLogic.GetModelsBy(s => s.Person_Id == currentStudent.Id);
                    if (studentLevels.Count == 0)
                    {
                        SetMessage("No StudentLevel Record!", Message.Category.Error);
                        ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelRoom), "Value", "Text");
                        return View(viewModel);
                    }

                    int maxLevelId = studentLevels.Max(sl => sl.Level.Id);
                    StudentLevel studentLevel = studentLevels.Where(sl => sl.Level.Id == maxLevelId).LastOrDefault();
                    //Session session = sessionLogic.GetModelBy(s => s.Activated == true);
                    Session session = sessionLogic.GetHostelSession();
                    viewModel.Session = session;

                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PaymentEtranzact paymentEtranzact = paymentEtranzactLogic.GetModelBy(p => p.ONLINE_PAYMENT.PAYMENT.Session_Id == session.Id &&
                                            p.ONLINE_PAYMENT.PAYMENT.Person_Id == currentStudent.Id && (p.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == 3 || p.ONLINE_PAYMENT.PAYMENT.Fee_Type_Id == 10));
                    if (paymentEtranzact == null)
                    {
                        RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                        RemitaPayment remitaPayment = remitaPaymentLogic.GetModelsBy(r => r.PAYMENT.Session_Id == session.Id && r.PAYMENT.Person_Id == currentStudent.Id &&
                                                       (r.PAYMENT.Fee_Type_Id == 3 || r.PAYMENT.Fee_Type_Id == 10) && r.Status.Contains("01")).LastOrDefault();

                        if (remitaPayment == null)
                        {
                            SetMessage("You have to pay school fees before making payment for hostel allocation!", Message.Category.Error);
                            ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelRoom), "Value", "Text");
                            return View(viewModel);
                        }
                    }

                    HostelAllocation existingHostelAllocation = hostelAllocationLogic.GetModelBy(ha => ha.Session_Id == session.Id && ha.Student_Id == currentStudent.Id);
                    if (existingHostelAllocation != null)
                    {
                        if (existingHostelAllocation.Occupied)
                        {
                            Payment payment = paymentLogic.GetModelBy(p => p.Person_Id == currentStudent.Id && p.Fee_Type_Id == existingHostelAllocation.Payment.FeeType.Id && p.Session_Id == existingHostelAllocation.Session.Id);
                            return RedirectToAction("HostelReceipt", new { Controller = "Hostel", Area = "Student", pmid = payment.Id });
                        }
                        else
                        {
                            Payment payment = paymentLogic.GetModelBy(p => p.Person_Id == currentStudent.Id && p.Fee_Type_Id == existingHostelAllocation.Payment.FeeType.Id && p.Session_Id == existingHostelAllocation.Session.Id);
                            Student.ViewModels.HostelViewModel existingstudentHostelViewModel = new Student.ViewModels.HostelViewModel();
                            existingstudentHostelViewModel.Payment = payment;
                            TempData["ViewModel"] = existingstudentHostelViewModel;
                            return RedirectToAction("Invoice", new { Controller = "Hostel", Area = "Student" });
                        }
                    }

                    if (currentStudent.Sex == null)
                    {
                        SetMessage("Error! Ensure that your student profile(Sex) is completely filled", Message.Category.Error);
                        ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelRoom), "Value", "Text");
                        return View(viewModel);
                    }

                    HostelAllocationCount hostelAllocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Sex_Id == currentStudent.Sex.Id && h.Level_Id == studentLevel.Level.Id);
                    if (hostelAllocationCount.Reserved == 0)
                    {
                        SetMessage("Error! The Set Number for reserved Bed Spaces has been exausted!", Message.Category.Error);
                        ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelRoom), "Value", "Text");
                        return View(viewModel);
                    }

                    HostelAllocation allocationCheck = hostelAllocationLogic.GetModelBy(h => h.Corner_Id == viewModel.HostelRoomCorner.Id && h.Hostel_Id == viewModel.HostelRoom.Hostel.Id && h.Room_Id == viewModel.HostelRoom.Id && h.Series_Id == viewModel.HostelRoom.Series.Id && h.Session_Id == session.Id);
                    if (allocationCheck != null)
                    {
                        SetMessage("Error! Bed Space has already been allocated!", Message.Category.Error);
                        ViewBag.CornerId = new SelectList(Utility.PopulateHostelRoomCorners(viewModel.HostelRoom), "Value", "Text");
                        return View(viewModel);
                    }

                    using (TransactionScope scope = new TransactionScope())
                    {
                        hostelAllocation.Corner = viewModel.HostelRoomCorner;
                        hostelAllocation.Hostel = viewModel.HostelRoom.Hostel;
                        hostelAllocation.Occupied = false;
                        hostelAllocation.Room = viewModel.HostelRoom;
                        hostelAllocation.Series = viewModel.HostelRoom.Series;
                        hostelAllocation.Session = session;
                        hostelAllocation.Student = currentStudent;
                        hostelAllocation.Person = person;

                        //Person person = personLogic.GetModelBy(p => p.Person_Id == currentStudent.Id);
                        viewModel.Person = person;

                        craetedPayment = CreatePayment(viewModel);
                        hostelAllocation.Payment = craetedPayment;

                        HostelAllocation newHostelAllocation = hostelAllocationLogic.Create(hostelAllocation);

                        hostelAllocationCount.Reserved -= 1;
                        hostelAllocationCount.TotalCount -= 1;
                        hostelAllocationCount.LastModified = DateTime.Now;
                        hostelAllocationCountLogic.Modify(hostelAllocationCount);

                        //Get Payment Specific Setting
                        RemitaSettings settings = new RemitaSettings();
                        RemitaSettingsLogic settingsLogic = new RemitaSettingsLogic();
                        settings = settingsLogic.GetBy((int)RemitaServiceSettings.Accomodation);

                        decimal amt = Convert.ToDecimal(craetedPayment.Amount);

                        //Get BaseURL
                        string remitaBaseUrl = ConfigurationManager.AppSettings["RemitaBaseUrl"].ToString();
                        RemitaPayementProcessor remitaProcessor = new RemitaPayementProcessor(settings.Api_key);
                        RemitaPayment remitaPayment = remitaProcessor.GenerateRRRCard(craetedPayment.InvoiceNumber, remitaBaseUrl, "ACCOMODATION FEES", settings, amt);

                        if (remitaPayment != null)
                        {
                            scope.Complete();
                        }
                    }

                    Student.ViewModels.HostelViewModel studentHostelViewModel = new Student.ViewModels.HostelViewModel();

                    studentHostelViewModel.Student = currentStudent;
                    studentHostelViewModel.Person = person;
                    studentHostelViewModel.StudentLevel = studentLevel;
                    studentHostelViewModel.Payment = craetedPayment;
                    TempData["ViewModel"] = studentHostelViewModel;
                    return RedirectToAction("Invoice", new { Controller = "Hostel", Area = "Student" });
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewReservedRooms");
        }

        public ActionResult ViewAllocationRequest()
        {
            try
            {
                viewModel = new HostelViewModel();
                ViewBag.Level = viewModel.LevelSelectListItem;
                ViewBag.Sessions = viewModel.SessionSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewAllocationRequest(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.Level != null)
                {
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    viewModel.HostelRequests = hostelRequestLogic.GetModelsBy(h => h.Level_Id == viewModel.Level.Id && h.Session_Id == viewModel.Session.Id && !h.Approved);
                    StudentLogic studentLogic = new StudentLogic();
                    for (int i = 0; i < viewModel.HostelRequests.Count; i++)
                    {
                        HostelRequest currentRequest = viewModel.HostelRequests[i];
                        if (viewModel.HostelRequests[i].Person != null)
                        {
                            viewModel.HostelRequests[i].Student = studentLogic.GetModelsBy(s => s.Person_Id == currentRequest.Person.Id).LastOrDefault();
                        }
                    }

                    if (viewModel.HostelRequests.Count == 0)
                    {
                        SetMessage("No requests! ", Message.Category.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Level = viewModel.LevelSelectListItem;
            ViewBag.Sessions = viewModel.SessionSelectListItem;

            TempData["viewModel"] = viewModel;

            return View(viewModel);
        }
        public ActionResult ApproveHostelRequest(HostelViewModel viewModel)
        {
            try
            {
                HostelViewModel existingViewModel = (HostelViewModel)TempData["viewModel"];
                if (viewModel.HostelRequests.Count > 0)
                {
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    HostelRequestCountLogic requestCountLogic = new HostelRequestCountLogic();
                    HostelAllocationCountLogic allocationCountLogic = new HostelAllocationCountLogic();

                    HostelRequestCount maleRequestCount = requestCountLogic.GetModelBy(a => a.Level_Id == existingViewModel.Level.Id && a.Sex_Id == (int)Sexes.Male && a.Approved);
                    HostelRequestCount femaleRequestCount = requestCountLogic.GetModelBy(a => a.Level_Id == existingViewModel.Level.Id && a.Sex_Id == (int)Sexes.Female && a.Approved);

                    HostelAllocationCount maleAllocationCount = allocationCountLogic.GetModelBy(h => h.Level_Id == existingViewModel.Level.Id && h.Sex_Id == (int)Sexes.Male && h.Activated);
                    HostelAllocationCount femaleAllocationCount = allocationCountLogic.GetModelBy(h => h.Level_Id == existingViewModel.Level.Id && h.Sex_Id == (int)Sexes.Female && h.Activated);

                    int approvalCountMale = 0;
                    int approvalCountFemale = 0;

                    bool maleReachedCount = false;
                    bool femaleReachedCount = false;

                    List<HostelRequest> requestsToApprove = viewModel.HostelRequests.Where(r => r.Approved).ToList();

                    for (int i = 0; i < requestsToApprove.Count; i++)
                    {
                        HostelRequest currentHostelRequest = requestsToApprove[i];
                        HostelRequest hostelRequest = hostelRequestLogic.GetModelBy(h => h.Hostel_Request_Id == currentHostelRequest.Id);

                        //List<HostelRequest> approvedHostelRequests = hostelRequestLogic.GetModelsBy(h => h.Level_Id == existingViewModel.Level.Id && h.Session_Id == existingViewModel.Session.Id && h.PERSON.Sex_Id == hostelRequest.Person.Sex.Id);

                        if (hostelRequest.Person.Sex.Id == (int)Sexes.Male)
                        {
                            if (maleRequestCount.TotalCount > 0 && (maleAllocationCount.Free > 0))
                            {
                                hostelRequest.Approved = currentHostelRequest.Approved;
                                hostelRequest.ApprovalDate = DateTime.Now;
                                hostelRequest.Expired = false;
                                hostelRequestLogic.Modify(hostelRequest);

                                maleRequestCount.TotalCount -= 1;
                                requestCountLogic.Modify(maleRequestCount);

                                approvalCountMale += 1;
                            }
                            else
                            {
                                maleReachedCount = true;
                            }
                        }
                        else if (hostelRequest.Person.Sex.Id == (int)Sexes.Female)
                        {
                            if (femaleRequestCount.TotalCount > 0 && femaleAllocationCount.Free > 0)
                            {
                                hostelRequest.Approved = currentHostelRequest.Approved;
                                hostelRequest.ApprovalDate = DateTime.Now;
                                hostelRequest.Expired = false;
                                hostelRequestLogic.Modify(hostelRequest);

                                femaleRequestCount.TotalCount -= 1;
                                requestCountLogic.Modify(femaleRequestCount);

                                approvalCountFemale += 1;
                            }
                            else
                            {
                                femaleReachedCount = true;
                            }
                        }
                    }

                    string message = "";

                    if (femaleReachedCount)
                    {
                        message += approvalCountFemale + " female hostel request(s) was/were approved, other female hostel request(s) was/were not approved because the set allocation count has been reached.";
                    }
                    else
                    {
                        message += approvalCountFemale + " female hostel request(s) was/were approved.";
                    }

                    if (maleReachedCount)
                    {
                        message += approvalCountMale + " male hostel request(s) was/were approved, other male hostel request(s) was/were not approved because the set allocation count has been reached.";
                    }
                    else
                    {
                        message += approvalCountMale + " male hostel request(s) was/were approved";
                    }

                    SetMessage(message, Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewAllocationRequest");
        }
        public JsonResult ApproveSingleHostelRequest(string requestId, string levelId, bool status)
        {
            JsonResultModel result = new JsonResultModel();
            try
            {
                if (!string.IsNullOrEmpty(requestId) && !string.IsNullOrEmpty(levelId))
                {
                    long hostelRequestId = Convert.ToInt64(requestId);
                    int selectedLevelId = Convert.ToInt32(levelId);
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    HostelAllocationCountLogic allocationCountLogic = new HostelAllocationCountLogic();
                    HostelRequestCountLogic requestCountLogic = new HostelRequestCountLogic();


                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PersonLogic personLogic = new PersonLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    UserLogic userLogic = new UserLogic();
                    var userName = User.Identity.Name;
                    var user = userLogic.GetModelsBy(u => u.User_Name == userName).FirstOrDefault();

                    HostelRequest hostelRequest = hostelRequestLogic.GetModelBy(h => h.Hostel_Request_Id == hostelRequestId);

                    HostelAllocationCount allocationCount = allocationCountLogic.GetModelBy(h => h.Level_Id == selectedLevelId && h.Sex_Id == hostelRequest.Person.Sex.Id && h.Activated);
                    HostelRequestCount requestCount = requestCountLogic.GetModelBy(h => h.Level_Id == selectedLevelId && h.Sex_Id == hostelRequest.Person.Sex.Id && h.Approved);

                    if (allocationCount.Free == 0 || requestCount.TotalCount <= 0)
                    {
                        result.IsError = true;
                        result.Message = "The set allocation/request count has been exhausted. Increase the allocation/request count or free up unoccupied allocations.";
                    }
                    else
                    {
                        var hostelAllocationCriteriaList = hostelAllocationCriteriaLogic.GetModelsBy(hac => hac.Level_Id == selectedLevelId && hac.HOSTEL.HOSTEL_TYPE.Hostel_Type_Name == hostelRequest.Person.Sex.Name &&
                                                        hac.HOSTEL_ROOM.Reserved == false && hac.HOSTEL_ROOM.Activated && hac.HOSTEL.Activated && hac.HOSTEL_SERIES.Activated &&
                                                        hac.HOSTEL_ROOM_CORNER.Activated);

                        if (hostelAllocationCriteriaList.Count == 0)
                        {
                            result.IsError = true;
                            result.Message = "Hostel Allocation Criteria for The Selected Level has not been set!.";

                        }
                        bool isModified = false;
                        for (int i = 0; i < hostelAllocationCriteriaList.Count; i++)
                        {
                            HostelAllocation hostelAllocation = new HostelAllocation();
                            hostelAllocation.Corner = hostelAllocationCriteriaList[i].Corner;
                            hostelAllocation.Hostel = hostelAllocationCriteriaList[i].Hostel;
                            hostelAllocation.Occupied = false;
                            hostelAllocation.Room = hostelAllocationCriteriaList[i].Room;
                            hostelAllocation.Series = hostelAllocationCriteriaList[i].Series;

                            HostelAllocation allocationCheck = hostelAllocationLogic.GetModelBy(h => h.Corner_Id == hostelAllocation.Corner.Id && h.Hostel_Id == hostelAllocation.Hostel.Id &&
                                                                h.Room_Id == hostelAllocation.Room.Id && h.Series_Id == hostelAllocation.Series.Id && h.Session_Id == hostelRequest.Session.Id);
                            if (allocationCheck != null)
                            {
                                continue;
                            }
                            using (TransactionScope scope = new TransactionScope())
                            {

                                hostelRequest.Approved = status;
                                hostelRequest.ApprovalDate = DateTime.Now;
                                hostelRequest.Expired = false;
                                hostelRequest.Hostel = hostelAllocation.Hostel;
                                hostelRequest.User = user;
                                hostelRequestLogic.Modify(hostelRequest);
                                if (status == true)
                                {
                                    requestCount.TotalCount -= 1;
                                    requestCountLogic.Modify(requestCount);
                                    isModified = true;
                                }
                                else
                                {
                                    requestCount.TotalCount += 1;
                                    requestCountLogic.Modify(requestCount);
                                    isModified = true;
                                }
                                scope.Complete();
                            }
                            result.IsError = false;
                            result.Message = "Hostel request status was modified.";
                            if (isModified == true)
                            {
                                break;
                            }
                            else
                            {
                                result.IsError = true;
                                result.Message = "There is no more available Bed space for Student Level.";
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetAllocationCount()
        {
            viewModel = new HostelViewModel();
            try
            {
                HostelAllocationCountLogic hostelAllocationCountLogic = new HostelAllocationCountLogic();
                SessionLogic sessionLogic = new SessionLogic();
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();

                Session session = sessionLogic.GetHostelSession();

                viewModel.HostelAllocationCounts = hostelAllocationCountLogic.GetAll();

                if (viewModel.HostelAllocationCounts != null && viewModel.HostelAllocationCounts.Count > 0)
                    viewModel.HostelAllocationCounts.ForEach(h =>
                    {
                        h.Allocated = hostelAllocationLogic.GetAllocationCount(session.Id, h.Level.Id, h.Sex.Id);
                    });
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SetAllocationCount(HostelViewModel viewModel)
        {
            HostelAllocationCountLogic hostelAllocationCountLogic = new HostelAllocationCountLogic();
            try
            {
                if (viewModel.HostelAllocationCounts != null)
                {
                    for (int i = 0; i < viewModel.HostelAllocationCounts.Count; i++)
                    {
                        HostelAllocationCount currentHostelAllocationCount = viewModel.HostelAllocationCounts[i];
                        if (Convert.ToInt32(currentHostelAllocationCount.Free) + Convert.ToInt32(currentHostelAllocationCount.Reserved) != Convert.ToInt32(currentHostelAllocationCount.TotalCount))
                        {
                            continue;
                        }
                        HostelAllocationCount hostelAllocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Hostel_Allocation_Count_Id == currentHostelAllocationCount.Id);

                        hostelAllocationCount.Free = currentHostelAllocationCount.Free;
                        hostelAllocationCount.LastModified = DateTime.Now;
                        hostelAllocationCount.Reserved = currentHostelAllocationCount.Reserved;
                        hostelAllocationCount.TotalCount = currentHostelAllocationCount.TotalCount;

                        hostelAllocationCountLogic.Modify(hostelAllocationCount);


                    }
                    SetMessage("Operation! Successful! ", Message.Category.Information);
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            viewModel = new HostelViewModel();
            viewModel.HostelAllocationCounts = hostelAllocationCountLogic.GetAll();
            return View(viewModel);
        }
        private Payment CreatePayment(HostelViewModel viewModel)
        {
            try
            {
                PaymentLogic paymentLogic = new PaymentLogic();
                OnlinePaymentLogic onlinePaymentLogic = new OnlinePaymentLogic();

                Payment newPayment = new Payment();

                PaymentMode paymentMode = new PaymentMode() { Id = 1 };
                PaymentType paymentType = new PaymentType() { Id = 2 };
                PersonType personType = viewModel.Person.Type;
                FeeType feeType = new FeeType() { Id = (int)FeeTypes.HostelFee };

                Payment payment = new Payment();
                payment.PaymentMode = paymentMode;
                payment.PaymentType = paymentType;
                payment.PersonType = personType;
                payment.FeeType = feeType;
                payment.DatePaid = DateTime.Now;
                payment.Person = viewModel.Person;
                payment.Session = viewModel.Session;

                Payment checkPayment = paymentLogic.GetModelBy(p => p.Person_Id == viewModel.Person.Id && p.Fee_Type_Id == feeType.Id && p.Session_Id == viewModel.Session.Id);
                if (checkPayment != null)
                {
                    newPayment = checkPayment;
                }
                else
                {
                    newPayment = paymentLogic.Create(payment);
                }

                OnlinePayment newOnlinePayment = null;

                if (newPayment != null)
                {
                    OnlinePayment onlinePaymentCheck = onlinePaymentLogic.GetModelBy(op => op.Payment_Id == newPayment.Id);
                    if (onlinePaymentCheck == null)
                    {
                        PaymentChannel channel = new PaymentChannel() { Id = (int)PaymentChannel.Channels.Etranzact };
                        OnlinePayment onlinePayment = new OnlinePayment();
                        onlinePayment.Channel = channel;
                        onlinePayment.Payment = newPayment;
                        newOnlinePayment = onlinePaymentLogic.Create(onlinePayment);
                    }

                }

                double amount = GetHostelFee(viewModel.HostelRoom.Hostel);

                HostelFeeLogic hostelFeeLogic = new HostelFeeLogic();
                HostelFee hostelFee = new HostelFee();

                HostelFee existingHostelFee = hostelFeeLogic.GetModelsBy(h => h.Payment_Id == newPayment.Id).LastOrDefault();

                if (existingHostelFee == null)
                {
                    hostelFee.Hostel = viewModel.HostelRoom.Hostel;
                    hostelFee.Payment = newPayment;
                    hostelFee.Amount = amount;

                    hostelFeeLogic.Create(hostelFee);
                }

                newPayment.Amount = amount.ToString();

                return newPayment;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private double GetHostelFee(Hostel hostel)
        {
            double amount = 0;
            try
            {
                string[] firstHostelGroup = { "KINGS PALACE", "KINGS ANNEX(A)", "KINGS ANNEX(B)", "ALUTA BASE", "ALUTA BASE(ANNEX)", "QUEENS PALACE(ANNEX)" };
                string[] secondHostelGroup = { "QUEENS PALACE I", "QUEENS PALACE II", "QUEENS PALACE III" };

                if (firstHostelGroup.Contains(hostel.Name))
                {
                    amount = 13000;
                }
                if (secondHostelGroup.Contains(hostel.Name))
                {
                    amount = 11500;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return amount;
        }
        //private string GetCornerName(int j)
        //{
        //    try
        //    {
        //        if (j == 0)
        //        {
        //            return "A";
        //        }
        //        if (j == 1)
        //        {
        //            return "B";
        //        }
        //        if (j == 2)
        //        {
        //            return "C";
        //        }
        //        if (j == 3)
        //        {
        //            return "D";
        //        }
        //        if (j == 4)
        //        {
        //            return "E";
        //        }
        //        if (j == 5)
        //        {
        //            return "F";
        //        }
        //        if (j == 6)
        //        {
        //            return "G";
        //        }
        //        if (j == 7)
        //        {
        //            return "H";
        //        }
        //        if (j == 8)
        //        {
        //            return "I";
        //        }
        //        if (j == 9)
        //        {
        //            return "J";
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return "";
        //}
        private string GetCornerName(int j)
        {
            try
            {
                string strAlpha = "";

                if (j == 0)
                {
                    j = 65;
                }
                else
                {
                    j += 65;
                }

                for (int i = j; i <= j; i++)
                {

                    strAlpha += ((char)i).ToString() + " ";
                    return strAlpha;
                }

            }
            catch (Exception)
            {
                throw;
            }
            return "";
        }
        public ActionResult ViewAllAllocationRequest()
        {
            try
            {
                viewModel = new HostelViewModel();
                ViewBag.Level = viewModel.LevelSelectListItem;
                ViewBag.Sessions = viewModel.SessionSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewAllAllocationRequest(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.Level != null)
                {
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    viewModel.HostelRequests = hostelRequestLogic.GetModelsBy(h => h.Level_Id == viewModel.Level.Id && h.Session_Id == viewModel.Session.Id);
                    StudentLogic studentLogic = new StudentLogic();
                    for (int i = 0; i < viewModel.HostelRequests.Count; i++)
                    {
                        HostelRequest currentRequest = viewModel.HostelRequests[i];
                        if (viewModel.HostelRequests[i].Person != null)
                        {
                            viewModel.HostelRequests[i].Student = studentLogic.GetModelsBy(s => s.Person_Id == currentRequest.Person.Id).LastOrDefault();
                        }
                    }

                    if (viewModel.HostelRequests.Count == 0)
                    {
                        SetMessage("No requests! ", Message.Category.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Level = viewModel.LevelSelectListItem;
            ViewBag.Sessions = viewModel.SessionSelectListItem;
            return View(viewModel);
        }

        public ActionResult EditAllocationRequest(int rid)
        {
            viewModel = new HostelViewModel();
            try
            {
                if (rid >= 0)
                {
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    HostelRequest hostelRequest = hostelRequestLogic.GetModelBy(h => h.Hostel_Request_Id == rid);

                    viewModel.HostelRequest = hostelRequest;
                    ViewBag.Level = new SelectList(viewModel.LevelSelectListItem, "Value", "Text", hostelRequest.Level.Id);
                    ViewBag.Session = new SelectList(viewModel.SessionSelectListItem, "Value", "Text", hostelRequest.Session.Id);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditAllocationRequest(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.HostelRequest != null)
                {
                    GeneralAudit generalAudit = new GeneralAudit();
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    HostelRequest hostelRequest = hostelRequestLogic.GetModelBy(h => h.Hostel_Request_Id == viewModel.HostelRequest.Id);
                    generalAudit.InitialValues = "Approved = " + hostelRequest.Approved + " Session = " + hostelRequest.Session.Id + " Level Id = " + hostelRequest.Level.Id;

                    hostelRequest.Approved = viewModel.HostelRequest.Approved;
                    hostelRequest.Session = viewModel.HostelRequest.Session;
                    hostelRequest.Level = viewModel.HostelRequest.Level;

                    hostelRequestLogic.Modify(hostelRequest);
                    generalAudit.Action = "Modify";


                    generalAudit.CurrentValues = "Approved = " + viewModel.HostelRequest.Approved + " Session = " + viewModel.HostelRequest.Session.Id + " Level Id = " + viewModel.HostelRequest.Level.Id;

                    generalAudit.Operation = "Modified Hostel Request for student " + hostelRequest.Student.MatricNumber;
                    generalAudit.TableNames = "Hostel Request";


                    generalAuditLogic.CreateGeneralAudit(generalAudit);
                    SetMessage("Operation! Successful! ", Message.Category.Information);


                    SetMessage("Operation Successful! ", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewAllAllocationRequest");
        }
        public ActionResult RemoveHostelRequest(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.HostelRequests.Count > 0)
                {
                    GeneralAudit generalAudit = new GeneralAudit();
                    GeneralAuditLogic generalAuditLogic = new GeneralAuditLogic();

                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    List<HostelRequest> requestsToRemove = viewModel.HostelRequests.Where(r => r.Remove).ToList();

                    for (int i = 0; i < requestsToRemove.Count; i++)
                    {
                        HostelRequest currentHostelRequest = requestsToRemove[i];
                        HostelRequest hostelRequest = hostelRequestLogic.GetModelBy(h => h.Hostel_Request_Id == currentHostelRequest.Id);
                        generalAudit.InitialValues = "Approved = " + hostelRequest.Approved + " Session = " + hostelRequest.Session.Id + " Level Id = " + hostelRequest.Level.Id;

                        hostelRequestLogic.Delete(h => h.Hostel_Request_Id == hostelRequest.Id);
                        generalAudit.Action = "Delete";
                        generalAudit.CurrentValues = "-";
                        generalAudit.Operation = "Deleted Hostel Request for student " + hostelRequest.Student.MatricNumber;
                        generalAudit.TableNames = "Hostel Request";


                        generalAuditLogic.CreateGeneralAudit(generalAudit);

                        SetMessage("Operation! Successful! ", Message.Category.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewAllAllocationRequest");
        }
        public ActionResult CompareAllocationCritereiaCount()
        {
            viewModel = new HostelViewModel();
            try
            {
                HostelAllocationCriteriaLogic allocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                HostelTypeLogic hostelTypeLogic = new HostelTypeLogic();
                LevelLogic levelLogic = new LevelLogic();
                HostelAllocationCountLogic allocationCountLogic = new HostelAllocationCountLogic();
                HostelSeriesLogic hostelSeriesLogic = new HostelSeriesLogic();
                HostelLogic hostelLogic = new HostelLogic();
                HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                SessionLogic sessionLogic = new SessionLogic();

                List<HostelType> hostelTypes = hostelTypeLogic.GetAll();
                List<Level> levels = levelLogic.GetAll();

                Session session = sessionLogic.GetHostelSession();

                List<DistinctAllocation> distinctAllocationList = new List<DistinctAllocation>();

                for (int i = 0; i < levels.Count; i++)
                {
                    Level currentLevel = levels[i];
                    List<HostelAllocationCriteria> allocationCriteriaList = new List<HostelAllocationCriteria>();
                    List<HostelAllocationCriteria> allocationCriteriaList1 = new List<HostelAllocationCriteria>();
                    allocationCriteriaList1 = allocationCriteriaLogic.GetModelsBy(a => a.Level_Id == currentLevel.Id);
                    List<int> distinctHostels = allocationCriteriaList1.Select(a => a.Hostel.Id).Distinct().ToList();
                    //allocationCriteriaList = allocationCriteriaList1.GroupBy(a => a.Hostel.Id).Last().Distinct().ToList();

                    for (int j = 0; j < distinctHostels.Count; j++)
                    {
                        int currentHostelId = distinctHostels[j];
                        Hostel hostel = hostelLogic.GetModelBy(h => h.Hostel_Id == currentHostelId);
                        List<HostelAllocationCriteria> allocationCriteriaList2 = new List<HostelAllocationCriteria>();

                        HostelType currentHostelType = hostel.HostelType;

                        List<HostelSeries> hostelSeries = hostelSeriesLogic.GetModelsBy(h => h.Hostel_Id == hostel.Id);
                        for (int m = 0; m < hostelSeries.Count; m++)
                        {
                            string RoomCorners = "";
                            string corners = "";
                            string series = "";
                            int usedCriteriaCount = 0;
                            int unusedCriteriaCount = 0;

                            HostelSeries currentSeries = hostelSeries[m];
                            allocationCriteriaList2 = allocationCriteriaLogic.GetModelsBy(a => a.Level_Id == currentLevel.Id && a.Hostel_Id == hostel.Id &&
                                                            a.HOSTEL.Hostel_Type_Id == currentHostelType.Hostel_Type_Id && a.Series_Id == currentSeries.Id);

                            for (int l = 0; l < allocationCriteriaList2.Count; l++)
                            {
                                HostelAllocationCriteria thisCriteria = allocationCriteriaList2[l];
                                HostelAllocation hostelAllocation = hostelAllocationLogic.GetModelsBy(h => h.Corner_Id == thisCriteria.Corner.Id && h.Room_Id == thisCriteria.Room.Id &&
                                                                            h.Hostel_Id == hostel.Id && h.Series_Id == currentSeries.Id && h.Session_Id == session.Id).LastOrDefault();
                                if (hostelAllocation != null)
                                {
                                    usedCriteriaCount += 1;
                                }
                                else
                                {
                                    unusedCriteriaCount += 1;
                                }

                                RoomCorners += " |" + allocationCriteriaList2[l].Room.Number + ": BedSpace " + allocationCriteriaList2[l].Corner.Name;
                            }

                            DistinctAllocation distinctAllocation = new DistinctAllocation();
                            HostelAllocationCount allocationCount = allocationCountLogic.GetModelsBy(a => a.Level_Id == currentLevel.Id && a.Sex_Id == currentHostelType.Hostel_Type_Id).LastOrDefault();
                            distinctAllocation.FreeAllocationCount = allocationCount.Free;
                            distinctAllocation.ReservedAllocationAccount = allocationCount.Reserved;
                            distinctAllocation.Level = currentLevel.Name;
                            distinctAllocation.Hostel = hostel.Name;
                            distinctAllocation.HostelType = currentHostelType.Hostel_Type_Name;
                            distinctAllocation.RoomCorner = RoomCorners;
                            distinctAllocation.Series = currentSeries.Name;
                            distinctAllocation.UnusedCriteriaCount = unusedCriteriaCount;
                            distinctAllocation.UsedCriteriaCount = usedCriteriaCount;
                            distinctAllocation.CriteriaCount = allocationCriteriaLogic.GetModelsBy(a => a.Level_Id == currentLevel.Id && a.Hostel_Id == hostel.Id &&
                                                                a.HOSTEL.Hostel_Type_Id == currentHostelType.Hostel_Type_Id && a.Series_Id == currentSeries.Id).Count;

                            distinctAllocationList.Add(distinctAllocation);
                        }
                    }
                }

                viewModel.DistinctAllocation = distinctAllocationList.OrderBy(a => a.Level).ThenBy(a => a.HostelType).ThenBy(a => a.Hostel).ThenBy(a => a.Series).ToList();
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        public ActionResult UpdateHostelRequest()
        {
            try
            {
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                List<HostelRequest> hostelRequests = hostelRequestLogic.GetAll();
                for (int i = 0; i < hostelRequests.Count; i++)
                {
                    if (hostelRequests[i].Student.MatricNumber.Contains("/12/") || hostelRequests[i].Student.MatricNumber.Contains("/13/") || hostelRequests[i].Student.MatricNumber.Contains("/14/") || hostelRequests[i].Student.MatricNumber.Contains("/15/"))
                    {
                        if (hostelRequests[i].Level.Id == 1)
                        {
                            hostelRequests[i].Level = new Level() { Id = 2 };
                        }
                        if (hostelRequests[i].Level.Id == 3)
                        {
                            hostelRequests[i].Level = new Level() { Id = 4 };
                        }
                    }
                    hostelRequestLogic.Modify(hostelRequests[i]);
                }
                SetMessage("Operation! Successful! ", Message.Category.Information);
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewAllAllocationRequest");
        }
        public JsonResult GetHostelSeries(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                int hostelId = Convert.ToInt32(id);
                HostelSeriesLogic hostelSeriesLogic = new HostelSeriesLogic();
                List<HostelSeries> hostelSeries = hostelSeriesLogic.GetModelsBy(hs => hs.Hostel_Id == hostelId);

                return Json(new SelectList(hostelSeries, "Id", "Name"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GetCorners(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                int seriesId = Convert.ToInt32(id);
                List<HostelRoomCorner> hostelRoomCorners = new List<HostelRoomCorner>();
                HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();

                List<string> corners = hostelRoomCornerLogic.GetEntitiesBy(c => c.HOSTEL_ROOM.Series_Id == seriesId).Select(c => c.Corner_Name).Distinct().ToList();
                foreach (var item in corners)
                {
                    hostelRoomCorners.Add(hostelRoomCornerLogic.GetModelsBy(hrc => hrc.Corner_Name == item).FirstOrDefault());
                }

                return Json(new MultiSelectList(hostelRoomCorners, "Id", "Name"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GetCornersByRoom(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                int roomId = Convert.ToInt32(id);
                List<HostelRoomCorner> hostelRoomCorners = new List<HostelRoomCorner>();
                HostelRoomCornerLogic hostelRoomCornerLogic = new HostelRoomCornerLogic();

                hostelRoomCorners = hostelRoomCornerLogic.GetModelsBy(c => c.Room_Id == roomId).ToList();

                return Json(new SelectList(hostelRoomCorners, "Id", "Name"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GetRooms(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                int seriesId = Convert.ToInt32(id);
                List<HostelRoom> hostelRooms = new List<HostelRoom>();
                HostelRoomLogic hostelRoomLogic = new HostelRoomLogic();

                hostelRooms = hostelRoomLogic.GetModelsBy(c => c.Series_Id == seriesId).ToList();

                return Json(new SelectList(hostelRooms, "Id", "Number"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult GetDepartments(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                Programme programme = new Programme() { Id = Convert.ToInt32(id) };

                DepartmentLogic departmentLogic = new DepartmentLogic();
                List<Department> departments = departmentLogic.GetBy(programme);

                return Json(new SelectList(departments, "Id", "Name"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult HostelRequestCount()
        {
            try
            {
                viewModel = new HostelViewModel();
                HostelRequestCountLogic hostelRequestCountLogic = new HostelRequestCountLogic();
                viewModel.HostelRequestCounts = hostelRequestCountLogic.GetAll();
            }
            catch (Exception ex)
            {

                SetMessage("Error Occured" + ex.Message, Message.Category.Error);
            }
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult HostelRequestCount(HostelViewModel viewModel)
        {
            HostelRequestCountLogic hostelRequestCountLogic = new HostelRequestCountLogic();
            try
            {

                if (viewModel.HostelRequestCounts != null)
                {
                    for (int i = 0; i < viewModel.HostelRequestCounts.Count; i++)
                    {
                        HostelRequestCount currentHostelRequestCount = viewModel.HostelRequestCounts[i];
                        //if (Convert.ToInt32(currentHostelAllocationCount.Free) + Convert.ToInt32(currentHostelAllocationCount.Reserved) != Convert.ToInt32(currentHostelAllocationCount.TotalCount))
                        //{
                        //    continue;
                        //}
                        HostelRequestCount hostelAllocationCount = hostelRequestCountLogic.GetModelBy(h => h.Hostel_Request_Count_Id == currentHostelRequestCount.Id);


                        hostelAllocationCount.LastModified = DateTime.Now;
                        hostelAllocationCount.TotalCount = currentHostelRequestCount.TotalCount;

                        hostelRequestCountLogic.Modify(hostelAllocationCount);


                    }
                    SetMessage("Operation! Successful! ", Message.Category.Information);
                }
            }
            catch (Exception ex)
            {

                SetMessage("Error Occured" + ex.Message, Message.Category.Error);
            }

            viewModel = new HostelViewModel();
            viewModel.HostelRequestCounts = hostelRequestCountLogic.GetAll();
            return View(viewModel);
        }
        public ActionResult ViewVacantBedSpaces()
        {
            try
            {
                viewModel = new HostelViewModel();
                ViewBag.Hostel = viewModel.HostelSelectListItem;
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult ViewVacantBedSpaces(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.Hostel != null)
                {
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    viewModel.HostelAllocations = hostelAllocationLogic.GetVacantBedSpaces(viewModel.Hostel);

                    if (viewModel.HostelAllocations == null || viewModel.HostelAllocations.Count <= 0)
                    {
                        SetMessage("No vacant bedspace.", Message.Category.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            ViewBag.Hostel = viewModel.HostelSelectListItem;
            return View(viewModel);
        }
        public ActionResult HostelBreakdown()
        {
            viewModel = new HostelViewModel();
            ViewBag.Session = viewModel.SessionSelectListItem;

            return View();
        }
        public JsonResult GetHostelAllocationBreakdown(int sessionId)
        {
            HostelBreakdownModel result = new HostelBreakdownModel();
            try
            {
                if (sessionId > 0)
                {
                    result.AllocationBreakdown = new List<HostelBreakdown>();

                    HostelAllocationCountLogic allocationCountLogic = new HostelAllocationCountLogic();
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();

                    List<HOSTEL_REQUEST> hostelRequests = hostelRequestLogic.GetEntitiesBy(h => h.Session_Id == sessionId);

                    List<HostelAllocationCount> allocationCounts = allocationCountLogic.GetAll();
                    allocationCounts.ForEach(h =>
                    {
                        HostelBreakdown hostelBreakdown = new HostelBreakdown();

                        hostelBreakdown.Allocated = hostelAllocationLogic.GetAllocationCount(sessionId, h.Level.Id, h.Sex.Id);
                        hostelBreakdown.Count = Convert.ToInt32(h.Free);
                        hostelBreakdown.Level = h.Level.Name;
                        hostelBreakdown.Requested = hostelRequests.Where(hr => hr.Level_Id == h.Level.Id && hr.PERSON.Sex_Id == h.Sex.Id) != null ?
                                                    hostelRequests.Where(hr => hr.Level_Id == h.Level.Id && hr.PERSON.Sex_Id == h.Sex.Id).Count() : 0;
                        hostelBreakdown.Approved = hostelRequests.Where(hr => hr.Level_Id == h.Level.Id && hr.PERSON.Sex_Id == h.Sex.Id && hr.Approved) != null ?
                                                    hostelRequests.Where(hr => hr.Level_Id == h.Level.Id && hr.PERSON.Sex_Id == h.Sex.Id && hr.Approved).Count() : 0;
                        hostelBreakdown.Sex = h.Sex.Name;

                        result.AllocationBreakdown.Add(hostelBreakdown);
                    });

                    result.IsError = false;
                }
                else
                {
                    result.IsError = true;
                    result.Message = "Invalid parameter.";
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.Message = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult HostelStatistics()
        {
            GetHostelCapacity();
            return View();
        }
        public void GetHostelCapacity()
        {
            HostelLogic hostelLogic = new HostelLogic();
            HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
            SessionLogic sessionLogic = new SessionLogic();
            HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
            int totalCapacity = 0;
            int capacityForFemale = 0;
            int capacityForMale = 0;
            int totalRequest = 0;
            int maleRequestCount = 0;
            int femaleRequestCount = 0;
            int totalApprovdRequest = 0;
            int femaleApprovedRequestCount = 0;
            int maleApprovedRequestCount = 0;
            int occupiedSpace = 0;
            var getcurrentHostelActiveSession = sessionLogic.GetModelsBy(d => d.Active_For_Hostel == true).LastOrDefault();

            if (getcurrentHostelActiveSession != null)
            {
                occupiedSpace = hostelAllocationLogic.GetModelsBy(u => u.Occupied == true && u.Session_Id == getcurrentHostelActiveSession.Id).Count;
                var allHostel = hostelLogic.GetModelsBy(t => t.Activated);
                ViewBag.HostelCount = allHostel.Count;
                var allHostelRequest = hostelRequestLogic.GetModelsBy(f => f.Approved == false && f.Session_Id == getcurrentHostelActiveSession.Id);
                var allHostelApprovedRequest = hostelRequestLogic.GetModelsBy(f => f.Approved == true && f.Session_Id == getcurrentHostelActiveSession.Id);
                if (allHostel.Count > 0)
                {
                    totalCapacity = allHostel.Select(h => h.Capacity).Sum();
                    capacityForMale = allHostel.Where(g => g.HostelType.Hostel_Type_Id == 1).Select(f => f.Capacity).Sum();
                    capacityForFemale = allHostel.Where(g => g.HostelType.Hostel_Type_Id == 2).Select(f => f.Capacity).Sum();
                    ViewBag.MaleHostelCapacity = capacityForMale;
                    ViewBag.FemaleHostelCapacity = capacityForFemale;
                    ViewBag.TotalCapacity = totalCapacity;
                }
                totalRequest = allHostelRequest.Count;
                ViewBag.TotalRequest = totalRequest;
                if (allHostelRequest.Count > 0)
                {
                    for (int i = 0; i < allHostelRequest.Count; i++)
                    {
                        var sexId = allHostelRequest[i].Person.Sex != null ? allHostelRequest[i].Person.Sex.Id : 0;
                        if (sexId == 1)
                        {
                            maleRequestCount += 1;
                        }
                        else
                        {
                            femaleRequestCount += 1;
                        }
                    }
                }
                totalApprovdRequest = allHostelApprovedRequest.Count;
                ViewBag.TotalApprovedRequest = totalApprovdRequest;

                if (allHostelApprovedRequest.Count > 0)
                {
                    for (int i = 0; i < allHostelApprovedRequest.Count; i++)
                    {
                        var sexId = allHostelApprovedRequest[i].Person.Sex != null ? allHostelApprovedRequest[i].Person.Sex.Id : 0;
                        if (sexId == 1)
                        {
                            maleApprovedRequestCount += 1;
                        }
                        else
                        {
                            femaleApprovedRequestCount += 1;
                        }
                    }
                }
            }
            else
            {
                ViewBag.MaleHostelCapacity = capacityForMale;
                ViewBag.FemaleHostelCapacity = capacityForFemale;
                ViewBag.TotalCapacity = totalCapacity;
                ViewBag.TotalRequest = totalRequest;
            }
            ViewBag.FemaleRequest = femaleRequestCount;
            ViewBag.MaleRequest = maleRequestCount;
            ViewBag.FemaleApprovedRequest = femaleApprovedRequestCount;
            ViewBag.MaleApprovedRequest = maleApprovedRequestCount;
            ViewBag.OccupiedSpace = occupiedSpace;
            if (totalApprovdRequest > 0)
            {
                ViewBag.PerFemaleApprovedRequest = (femaleApprovedRequestCount) * 100 / (totalApprovdRequest);
                ViewBag.PerMaleApprovedRequest = (maleApprovedRequestCount) * 100 / (totalApprovdRequest);
            }
            else
            {
                ViewBag.PerFemaleApprovedRequest = 0;
                ViewBag.PerMaleApprovedRequest = 0;
            }
            if (totalRequest > 0)
            {
                ViewBag.PerFemaleRequest = 0;
                ViewBag.PerMaleRequest = 0;
            }
            else
            {
                ViewBag.PerFemaleRequest = 0;
                ViewBag.PerMaleRequest = 0;
            }
            if (totalCapacity > 0)
            {
                ViewBag.PerOccupiedSpace = (occupiedSpace) * 100 / (totalCapacity);
                ViewBag.PerFemaleCapacity = (capacityForFemale) * 100 / (totalCapacity);
                ViewBag.PerMaleCapacity = (capacityForMale) * 100 / (totalCapacity);
            }
            else
            {
                ViewBag.PerOccupiedSpace = 0;
                ViewBag.PerFemaleCapacity = 0;
                ViewBag.PerMaleCapacity = 0;
            }





        }
        public ActionResult ViewHostelPendingRequest()
        {
            try
            {
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                SessionLogic sessionLogic = new SessionLogic();
                var getcurrentHostelActiveSession = sessionLogic.GetModelsBy(d => d.Active_For_Hostel == true).LastOrDefault();
                viewModel.HostelRequests = hostelRequestLogic.GetModelsBy(h =>h.Session_Id == getcurrentHostelActiveSession.Id && h.Approved == false && h.Reason==null && h.Expired==false).OrderBy(d=>d.RequestDate).ToList();

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }
            return View(viewModel);
        }
        public ActionResult ViewHostels()
        {
            try
            {
                HostelLogic hostelLogic = new HostelLogic();
                hostelLogic.GetAll();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }
        public ActionResult AllCurrentAllocations()
        {
            try
            {
                SessionLogic sessionLogic = new SessionLogic();
                var getcurrentHostelActiveSession = sessionLogic.GetModelsBy(d => d.Active_For_Hostel == true).LastOrDefault();
                if (getcurrentHostelActiveSession != null)
                {
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    viewModel.HostelAllocations = hostelAllocationLogic.GetModelsBy(h => h.Session_Id == getcurrentHostelActiveSession.Id && h.Occupied==true);
                    //StudentLogic studentLogic = new StudentLogic();
                    //for (int i = 0; i < viewModel.HostelAllocations.Count; i++)
                    //{
                    //    HostelAllocation currentAllocation = viewModel.HostelAllocations[i];
                    //    if (viewModel.HostelAllocations[i].Person != null)
                    //    {
                    //        viewModel.HostelAllocations[i].Student = studentLogic.GetModelsBy(s => s.Person_Id == currentAllocation.Person.Id).LastOrDefault();
                    //    }
                    //}
                }

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return View(viewModel);
        }
        public ActionResult ReactivateRequest()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ReactivateRequest(HostelViewModel viewModel)
        {
            try
            {
                if (viewModel.RemitaPayment.RRR != null)
                {
                    //PaymentEtranzactLogic paymentEtranzactLogic = new PaymentEtranzactLogic();
                    PaymentLogic paymentLogic = new PaymentLogic();
                    HostelAllocationLogic hostelAllocationLogic = new HostelAllocationLogic();
                    RemitaPaymentLogic remitaPaymentLogic = new RemitaPaymentLogic();
                    SessionLogic sessionLogic = new SessionLogic();
                    HostelAllocationCountLogic hostelAllocationCountLogic = new HostelAllocationCountLogic();
                    HostelAllocationCriteriaLogic hostelAllocationCriteriaLogic = new HostelAllocationCriteriaLogic();
                    List<HostelAllocationCriteria> hostelAllocationCriteriaList = new List<HostelAllocationCriteria>();
                    StudentLevelLogic studentLevelLogic = new StudentLevelLogic();
                    HostelAllocation hostelAllocation = new HostelAllocation();
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    HostelRequestCountLogic hostelRequestCountLogic = new HostelRequestCountLogic();


                    Payment payment = null;
                    RemitaPayment remitaPayment = new RemitaPayment();
                    RemitaPayementProcessor remitaPayementProcessor = new RemitaPayementProcessor("918567");
                    remitaPayment = remitaPaymentLogic.GetModelsBy(a => a.RRR == viewModel.RemitaPayment.RRR).FirstOrDefault();

                    if (remitaPayment != null)
                    {
                        remitaPayment = remitaPayementProcessor.GetStatus(remitaPayment.OrderId);
                        if (remitaPayment.Status.Contains("01:") || remitaPayment.Description.Contains("Manual"))
                        {
                            payment = remitaPayment.payment;
                        }
                        else
                        {
                            SetMessage("Payment was not Successfull", Message.Category.Error);
                            return View();
                        }
                        var existingAllocation=hostelAllocationLogic.GetModelBy(d => d.Payment_Id == payment.Id && d.Occupied==true);
                        if (existingAllocation != null)
                        {
                            SetMessage("This PAyment Has Already been Allocated a Room", Message.Category.Information);
                            return View();
                        }
                        var request = hostelRequestLogic.GetModelsBy(d => d.Person_Id == payment.Person.Id && d.Payment_Id == payment.Id).LastOrDefault();
                        HostelAllocationCount allocationCount = hostelAllocationCountLogic.GetModelBy(h => h.Level_Id == request.Level.Id && h.Sex_Id == payment.Person.Sex.Id && h.Activated);
                        HostelRequestCount requestCount = hostelRequestCountLogic.GetModelBy(h => h.Level_Id == request.Level.Id && h.Sex_Id == payment.Person.Sex.Id && h.Approved);

                        if (allocationCount.Free == 0 || requestCount.TotalCount <= 0)
                        {
                            SetMessage("The set allocation/request count has been exhausted. Increase the allocation/request count", Message.Category.Error);
                            return View();
                        }
                        else
                        {
                            hostelAllocationCriteriaList = hostelAllocationCriteriaLogic.GetModelsBy(hac => hac.Level_Id == request.Level.Id && hac.HOSTEL.HOSTEL_TYPE.Hostel_Type_Name == payment.Person.Sex.Name &&
                                                            hac.HOSTEL_ROOM.Reserved == false && hac.HOSTEL_ROOM.Activated && hac.HOSTEL.Activated && hac.HOSTEL_SERIES.Activated &&
                                                            hac.HOSTEL_ROOM_CORNER.Activated);

                            if (hostelAllocationCriteriaList.Count == 0)
                            {
                                SetMessage("Hostel Allocation Criteria for The Selected Level has not been set!.", Message.Category.Error);
                                return View();

                            }
                            int SpaceForAmountPaid = 0;
                            var amountPaid = NewGetHostelFee(request.Hostel);
                            if (hostelAllocationCriteriaList.Count > 0)
                            {
                                for (int i = 0; i < hostelAllocationCriteriaList.Count; i++)
                                {
                                    var hostel = hostelAllocationCriteriaList[i].Hostel;
                                    var amount = NewGetHostelFee(hostel);
                                    if (amount <= amountPaid)
                                    {
                                        SpaceForAmountPaid += 1;
                                    }
                                }
                            }
                            if (SpaceForAmountPaid > 0)
                            {
                                request.Expired = false;
                                request.Approved = true;
                                request.Reason = "Reactivated";
                                request.ApprovalDate = DateTime.Now;
                                hostelRequestLogic.Modify(request);
                                SetMessage("Operation Successfull You can Proceed with Printing of Receipt within the next 72hours ", Message.Category.Information);
                                return View();
                            }
                            else
                            {
                                SetMessage("Hostel Space of Same Amount Paid is not available", Message.Category.Error);
                                return View();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }
        public ActionResult ApproveHostelRequestFromViewPendingRequest(HostelViewModel viewModel)
        {
            try
            {
               

                HostelViewModel existingViewModel = (HostelViewModel)TempData["viewModel"];
                if (viewModel.HostelRequests.Count > 0)
                {
                    HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                    HostelRequestCountLogic requestCountLogic = new HostelRequestCountLogic();
                    HostelAllocationCountLogic allocationCountLogic = new HostelAllocationCountLogic();
                    UserLogic userLogic = new UserLogic();
                    var userName = User.Identity.Name;
                    var user=userLogic.GetModelsBy(u => u.User_Name == userName).FirstOrDefault();

                    int approvalCountMale = 0;
                    int approvalCountFemale = 0;

                    bool maleReachedCount = false;
                    bool femaleReachedCount = false;

                    List<HostelRequest> requestsToApprove = viewModel.HostelRequests.Where(r => r.Approved).ToList();

                    for (int i = 0; i < requestsToApprove.Count; i++)
                    {
                        HostelRequest currentHostelRequest = requestsToApprove[i];
                        HostelRequest hostelRequest = hostelRequestLogic.GetModelBy(h => h.Hostel_Request_Id == currentHostelRequest.Id);

                        HostelRequestCount maleRequestCount = requestCountLogic.GetModelBy(a => a.Level_Id == hostelRequest.Level.Id && a.Sex_Id == (int)Sexes.Male && a.Approved);
                        HostelRequestCount femaleRequestCount = requestCountLogic.GetModelBy(a => a.Level_Id == hostelRequest.Level.Id && a.Sex_Id == (int)Sexes.Female && a.Approved);

                        HostelAllocationCount maleAllocationCount = allocationCountLogic.GetModelBy(h => h.Level_Id == hostelRequest.Level.Id && h.Sex_Id == (int)Sexes.Male && h.Activated);
                        HostelAllocationCount femaleAllocationCount = allocationCountLogic.GetModelBy(h => h.Level_Id == hostelRequest.Level.Id && h.Sex_Id == (int)Sexes.Female && h.Activated);
                        //List<HostelRequest> approvedHostelRequests = hostelRequestLogic.GetModelsBy(h => h.Level_Id == existingViewModel.Level.Id && h.Session_Id == existingViewModel.Session.Id && h.PERSON.Sex_Id == hostelRequest.Person.Sex.Id);
                        if (hostelRequest.Approved == false)
                        {
                            if (hostelRequest.Person.Sex.Id == (int)Sexes.Male)
                            {
                                if (maleRequestCount.TotalCount > 0 && (maleAllocationCount.Free > 0))
                                {
                                    hostelRequest.Approved = currentHostelRequest.Approved;
                                    hostelRequest.ApprovalDate = DateTime.Now;
                                    hostelRequest.Expired = false;
                                    hostelRequest.User = user;
                                    hostelRequestLogic.Modify(hostelRequest);

                                    maleRequestCount.TotalCount -= 1;
                                    requestCountLogic.Modify(maleRequestCount);

                                    approvalCountMale += 1;
                                }
                                else
                                {
                                    maleReachedCount = true;
                                }
                            }
                            else if (hostelRequest.Person.Sex.Id == (int)Sexes.Female)
                            {
                                if (femaleRequestCount.TotalCount > 0 && femaleAllocationCount.Free > 0)
                                {
                                    hostelRequest.Approved = currentHostelRequest.Approved;
                                    hostelRequest.ApprovalDate = DateTime.Now;
                                    hostelRequest.Expired = false;
                                    hostelRequest.User = user;
                                    hostelRequestLogic.Modify(hostelRequest);

                                    femaleRequestCount.TotalCount -= 1;
                                    requestCountLogic.Modify(femaleRequestCount);

                                    approvalCountFemale += 1;
                                }
                                else
                                {
                                    femaleReachedCount = true;
                                }
                            }
                        }
                        else if (hostelRequest.Approved == true)
                        {
                            if (hostelRequest.Person.Sex.Id == (int)Sexes.Male)
                            {
                                    approvalCountMale += 1;

                            }
                            else if (hostelRequest.Person.Sex.Id == (int)Sexes.Female)
                            {


                                    approvalCountFemale += 1;
 
                            }
                        }

                    }

                    string message = "";

                    if (femaleReachedCount)
                    {
                        message += approvalCountFemale + " female hostel request(s) was/were approved, other female hostel request(s) was/were not approved because the set allocation count has been reached.";
                    }
                    else
                    {
                        message += approvalCountFemale + " female hostel request(s) was/were approved.";
                    }

                    if (maleReachedCount)
                    {
                        message += approvalCountMale + " male hostel request(s) was/were approved, other male hostel request(s) was/were not approved because the set allocation count has been reached.";
                    }
                    else
                    {
                        message += approvalCountMale + " male hostel request(s) was/were approved";
                    }

                    SetMessage(message, Message.Category.Information);
                }
            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }

            return RedirectToAction("ViewHostelPendingRequest");
        }
        public ActionResult AllApprovedHostelRequest()
        {
            try
            {
                HostelRequestLogic hostelRequestLogic = new HostelRequestLogic();
                SessionLogic sessionLogic = new SessionLogic();
                var getcurrentHostelActiveSession = sessionLogic.GetModelsBy(d => d.Active_For_Hostel == true).LastOrDefault();
                viewModel.HostelRequests = hostelRequestLogic.GetModelsBy(h =>h.Approved == true && h.Session_Id == getcurrentHostelActiveSession.Id );

            }
            catch (Exception ex)
            {
                SetMessage("Error! " + ex.Message, Message.Category.Error);
            }
            return View(viewModel);
        }
        private double NewGetHostelFee(Hostel hostel)
        {
            double amount = 0;
            HostelAmountLogic hostelAmountLogic = new HostelAmountLogic();
            HostelAmount hostelAmount = new HostelAmount();
            try
            {
                hostelAmount = hostelAmountLogic.GetModelBy(p => p.Hostel_Id == hostel.Id);

                amount = Convert.ToDouble(hostelAmount.Amount);
            }
            catch (Exception)
            {
                throw;
            }

            return amount;
        }

    }

}