﻿using Abp.ObjectMapping;
using Abp.UI;
using IEManageSystem.CMS.DomainModel.PageDatas;
using IEManageSystem.CMS.DomainModel.Pages;
using IEManageSystem.CMS.Repositorys;
using IEManageSystem.Dtos.CMS;
using IEManageSystem.Services.ManageHome.CMS.PageQuerys.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IEManageSystem.Services.ManageHome.CMS.PageQuerys
{
    public class PageQueryAppService: IEManageSystemAppServiceBase, IPageQueryAppService
    {
        private readonly IObjectMapper _objectMapper;

        private PageManager _pageManager { get; set; }

        private PageDataManager _pageDataManager { get; set; }

        private IPageRepository _repository => _pageManager.PageRepository;

        public PageQueryAppService(
            IObjectMapper objectMapper,
            PageManager pageManager,
            PageDataManager pageDataManager
            )
        {
            _objectMapper = objectMapper;

            _pageManager = pageManager;

            _pageDataManager = pageDataManager;
        }

        public GetPagesOutput GetPages(GetPagesInput input)
        {
            IEnumerable<PageBase> pages = string.IsNullOrEmpty(input.SearchKey) ?
                _repository.GetAll() :
                GetPagesForSearchKey(input.SearchKey);

            int pageNum = pages.Count();

            pages = pages.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize);

            return new GetPagesOutput()
            {
                ResourceNum = pageNum,
                PageIndex = input.PageIndex,
                Pages = CreatePageDtos(pages.ToList())
            };
        }

        private List<PageDto> CreatePageDtos(List<PageBase> pageBases)
        {
            List<PageDto> pageDtos = new List<PageDto>();

            foreach (var page in pageBases)
            {
                var pageDto = new PageDto();
                pageDto.Id = page.Id;
                pageDto.Name = page.Name;
                pageDto.DisplayName = page.DisplayName;
                pageDto.Description = page.Description;

                if (page is StaticPage)
                {
                    pageDto.PageType = "StaticPage";
                }
                else if (page is ContentPage)
                {
                    pageDto.PageType = "ContentPage";
                }

                pageDtos.Add(pageDto);
            }

            return pageDtos;
        }

        private IEnumerable<PageBase> GetPagesForSearchKey(string searchKey)
        {
            return _repository.GetAll().Where(e =>
                e.DisplayName.Contains(searchKey) || e.Name.Contains(searchKey)
            );
        }

        public GetPageOutput GetPage(GetPageInput input)
        {
            PageBase page = null;
            if (input.Id != null)
            {
                page = _repository.FirstOrDefault(input.Id.Value);
            }

            if (page == null && !string.IsNullOrWhiteSpace(input.Name))
            {
                page = _repository.FirstOrDefault(item => item.Name == input.Name);
            }

            if (page == null)
            {
                return new GetPageOutput() { Page = null };
            }

            return new GetPageOutput() { Page = _objectMapper.Map<PageDto>(page) };
        }

        public GetPageComponentOutput GetPageComponent(GetPageComponentInput input)
        {
            List<PageComponentDto> dtos = new List<PageComponentDto>();
            foreach (var item in _pageManager.GetPageComponents(input.Name))
            {
                dtos.Add(CreatePageComponentDto(item));
            }

            return new GetPageComponentOutput() { PageComponents = dtos };
        }

        private PageComponentDto CreatePageComponentDto(PageComponentBase page)
        {
            PageComponentDto dto = new PageComponentDto();

            dto.Name = page.Name;
            dto.Sign = page.Sign;
            dto.ParentSign = page.ParentSign;
            dto.Col = page.Col;
            dto.Height = page.Height;
            dto.Padding = page.Padding;
            dto.Margin = page.Margin;
            dto.BackgroundColor = page.BackgroundColor;
            dto.ClassName = page.ClassName;
            dto.PageComponentSettings = new List<PageComponentSettingDto>();

            foreach (var item in page.PageComponentSettings) {
                PageComponentSettingDto pageComponentSetting = new PageComponentSettingDto() {
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    Field1 = item.Field1,
                    Field2 = item.Field2,
                    Field3  = item.Field3,
                    Field4 = item.Field4,
                    Field5 = item.Field5
                };

                dto.PageComponentSettings.Add(pageComponentSetting);
            }

            if (page is CompositeComponent)
            {
                dto.ComponentType = "CompositeComponent";
            }
            else if (page is PageLeafComponent)
            {
                var pageLeafComponent = (PageLeafComponent)page;

                dto.TargetPageId = pageLeafComponent.TargetPageId;
                dto.ComponentType = "PageLeafComponent";
            }
            else
            {
                dto.ComponentType = "ContentLeafComponent";
            }

            return dto;
        }

        /// <summary>
        /// 获取页面文章
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public GetPageDatasOutput GetPageDatas(GetPageDatasInput input)
        {
            PageBase page = null;
            if (!string.IsNullOrWhiteSpace(input.PageName))
            {
                page = _repository.GetAllIncluding(e => e.PageDatas).FirstOrDefault(e => e.Name == input.PageName);
            }

            if (page == null && input.Id != null)
            {
                page = _repository.GetAllIncluding(e => e.PageDatas).FirstOrDefault(e => e.Id == input.Id);
            }

            if (page == null)
            {
                throw new UserFriendlyException("获取文章列表失败，未找到页面");
            }

            return new GetPageDatasOutput()
            {
                PageDatas = _objectMapper.Map<List<PageDataDto>>(page.PageDatas),
                ResourceNum = page.PageDatas.Count,
                PageIndex = input.PageIndex
            };
        }

        public GetComponentDataOutput GetComponentDatas(GetComponentDataInput input)
        {
            var pageData = _pageDataManager.GetPageDataIncludeAllProperty(input.PageName, input.PageDataName);

            return new GetComponentDataOutput()
            {
                ComponentDatas = _objectMapper.Map<List<ContentComponentDataDto>>(pageData.ContentComponentDatas)
            };
        }
    }
}